using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Windows.Forms;
using System.ComponentModel;
using NLog;
using Cornerstone.Database.Tables;
using System.Threading;
using System.Reflection;
using Cornerstone.Database.CustomTypes;
using System.Collections.ObjectModel;

namespace Cornerstone.Database {
    public abstract class SettingsManager: Dictionary<string, DBSetting> {
        #region Private Variables

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DatabaseManager dbManager;
        private Dictionary<string, PropertyInfo> propertyLookup;
        private Dictionary<PropertyInfo, CornerstoneSettingAttribute> attributeLookup;

        private bool initializing;

        #endregion

        public ReadOnlyCollection<DBSetting> AllSettings {
            get {
                return _allSettings.AsReadOnly();
            }
        } protected List<DBSetting> _allSettings;

        /// <summary>
        /// Fires every time a settings value has been changed.
        /// </summary>
        public event SettingChangedDelegate SettingChanged;

        public SettingsManager(DatabaseManager dbManager) {
            this.dbManager = dbManager;

            initializing = true;
            
            BuildPropertyLookup();
            LoadSettingsFromDatabase();
            UpdateAndSyncSettings();

            initializing = false;

            logger.Info("SettingsManager Created");
        }

        protected void Sync(SettingsManager otherSettings) {
            _allSettings.AddRange(otherSettings.AllSettings);

            foreach(DBSetting currSetting in otherSettings.Values) {
                if (!this.ContainsKey(currSetting.Key))
                    Add(currSetting.Key, currSetting);
            }
        }

        private void generate() {
            string settings = "\n\n";
            string settings2 = "\n\n";

            foreach (DBSetting currSetting in dbManager.Get<DBSetting>(null)) {
                string def;

                if (currSetting.Type == "String") {
                    def = "\"" + currSetting.Value + "\"";
                }
                else
                    def = currSetting.Value.ToString();

                string propertyName = currSetting.Name;
                while (propertyName.Contains(" ")) 
                    propertyName = currSetting.Name.Replace(" ", "");

                string privateName = propertyName.Substring(1);
                privateName = "_" + char.ToLower(propertyName[0]) + privateName;


                settings += "        [CornerstoneSetting(\n";
                settings += "            Name = \"" + currSetting.Name + "\",\n";
                settings += "            Description = \"" + currSetting.Description + "\",\n";
                settings += "            Groups = \"" + currSetting.Grouping.ToString() + "\",\n";
                settings += "            Identifier = \"" + currSetting.Key + "\",\n";
                settings += "            Default = \"" + def + "\")]\n";
                settings += "        public " + currSetting.Type.ToString().ToLower() + " " + propertyName + " {\n";
                settings += "            get { return " + privateName + "; }\n";
                settings += "            set {\n";
                settings += "                " + privateName + " = value;\n";
                settings += "                OnSettingChanged(\"" + currSetting.Key + "\");\n";
                settings += "            }\n";
                settings += "        }\n";
                settings += "        private " + currSetting.Type.ToString().ToLower() + " " + privateName + ";\n\n\n";

                settings2 += currSetting.Grouping.ToString() + "\t" + currSetting.Name + " (" + currSetting.Key + ")\n";
            }
            logger.Info(settings);
            logger.Info(settings2);

        }


        /// <summary>
        /// This method should be called by the super class when a setting has been changed.
        /// </summary>
        /// <param name="settingIdentifier">
        /// The identifier as defined in the attribute for the setting property in the 
        /// super class.
        /// </param>
        public void OnSettingChanged(string settingIdentifier) {
            // if we are intializing, ignore all changes
            if (initializing)
                return;
            
            // make sure we have been passed a valid identifier
            if (!propertyLookup.ContainsKey(settingIdentifier)) {
                logger.Error("Invalid call to OnSettingChanged with \"" + settingIdentifier + "\" identifier!");
                return;
            }

            // grab property and setting info
            PropertyInfo property = propertyLookup[settingIdentifier];
            DBSetting setting = this[settingIdentifier];

            // update the setting and commit
            object oldValue = setting.Value;
            setting.Value = property.GetGetMethod().Invoke(this, null);
            setting.Commit();

            // notify any listeners of the value change
            if (SettingChanged != null)
                SettingChanged(setting, oldValue);
        }

        /// <summary>
        /// Stores property and attribute info from the super class for quick lookup later.
        /// </summary>
        private void BuildPropertyLookup() {
            propertyLookup = new Dictionary<string, PropertyInfo>();
            attributeLookup = new Dictionary<PropertyInfo, CornerstoneSettingAttribute>();

            foreach (PropertyInfo currProperty in GetType().GetProperties()) {
                // make sure this property is intended to be a setting
                object[] attributes = currProperty.GetCustomAttributes(typeof(CornerstoneSettingAttribute), true);
                if (attributes.Length == 0)
                    continue;

                // and store it's info for quick access later
                CornerstoneSettingAttribute attribute = attributes[0] as CornerstoneSettingAttribute;
                propertyLookup[attribute.Identifier] = currProperty;
                attributeLookup[currProperty] = attribute;
            }
        }

        /// <summary>
        /// Loads all existing settings from the database.
        /// </summary>
        private void LoadSettingsFromDatabase() {
            List<DBSetting> settingList = dbManager.Get<DBSetting>(null);
            foreach (DBSetting currSetting in settingList) {
                try {
                    this.Add(currSetting.Key, currSetting);
                    currSetting.SettingsManager = this;
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    logger.Error("Error loading setting " + currSetting.Name + " (key = " + currSetting.Key + ")");
                }
            }
        }

        /// <summary>
        /// Populates properties in super class from data in the database, and adds new properties 
        /// defined in the super class to the database.
        /// </summary>
        private void UpdateAndSyncSettings() {
            _allSettings = new List<DBSetting>();

            foreach (PropertyInfo currProperty in propertyLookup.Values) {
                try {
                    SyncSetting(currProperty);
                    
                    CornerstoneSettingAttribute attribute = attributeLookup[currProperty];
                    DBSetting setting = this[attribute.Identifier];

                    currProperty.GetSetMethod().Invoke(this, new Object[] { setting.Value });

                    _allSettings.Add(setting);
                }
                catch (Exception e) {
                    if (e is ThreadAbortException)
                        throw e;

                    logger.ErrorException("Failed loading setting data for " + currProperty.Name + ".", e);
                }
            }
        }

        private void SyncSetting(PropertyInfo property) {
            CornerstoneSettingAttribute attribute = attributeLookup[property];
            StringList groups = new StringList(attribute.Groups);

            if (!this.ContainsKey(attribute.Identifier)) {
                DBSetting newSetting = new DBSetting();
                newSetting.Key = attribute.Identifier;
                newSetting.Name = attribute.Name;
                newSetting.Value = attribute.Default;
                newSetting.Type = DBSetting.TypeLookup(property.PropertyType);
                newSetting.Description = attribute.Description;
                newSetting.Grouping.AddRange(groups);
                newSetting.DBManager = this.dbManager;
                newSetting.Commit();

                this[attribute.Identifier] = newSetting;
            } else {
                DBSetting existingSetting = this[attribute.Identifier];
                
                // update name if neccisary
                if (!existingSetting.Name.Equals(attribute.Name))
                    existingSetting.Name = attribute.Name;

                // update description if neccisary
                if (!existingSetting.Description.Equals(attribute.Description))
                    existingSetting.Description = attribute.Description;
                
                // update groups if neccisary
                bool reloadGrouping = false;
                if (existingSetting.Grouping.Count != groups.Count)
                    reloadGrouping = true;
                else
                    for (int i = 0; i < existingSetting.Grouping.Count; i++) {
                        if (i >= groups.Count || !existingSetting.Grouping[i].Equals(groups[i])) {
                            reloadGrouping = true;
                            break;
                        }
                    }

                if (reloadGrouping) {
                    existingSetting.Grouping.Clear();
                    existingSetting.Grouping.AddRange(groups);
                }

                existingSetting.Commit();
            }
        }
    }

    public delegate void SettingChangedDelegate(DBSetting setting, object oldValue);

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CornerstoneSettingAttribute : Attribute {
        public string Identifier { get; set; }
        public string Groups { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public object Default { get; set; }
    }
}
