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

namespace Cornerstone.Database {
    public class SettingsManager: Dictionary<string, DBSetting> {
        #region Private Variables

        private DatabaseManager DBManager;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        #endregion

        #region Private Methods

        private void commit() {
            foreach (DBSetting currSetting in this.Values) {
                if (currSetting.CommitNeeded) 
                    DBManager.Commit(currSetting);
            }
        }

        private void processChildNodes(XmlNode parentNode, List<string> parentGroups) {
            if (parentGroups == null)
                parentGroups = new List<string>();

            int newSettingCount = 0;
            foreach (XmlNode currNode in parentNode.ChildNodes) {
                try {
                    if (currNode.Name == "group") {
                        parentGroups.Add(currNode.Attributes["name"].Value);
                        processChildNodes(currNode, parentGroups);
                        parentGroups.Remove(currNode.Attributes["name"].Value);
                    }

                    if (currNode.Name == "setting") {
                        string idString = "";
                        string name = "";
                        string description = "";
                        string value = "";
                        string type = "";

                        // try to load via inline values

                        if (currNode.Attributes["id"] != null)
                            idString = currNode.Attributes["id"].Value;

                        if (currNode.Attributes["name"] != null)
                            name = currNode.Attributes["name"].Value;

                        if (currNode.Attributes["description"] != null)
                            description = currNode.Attributes["description"].Value;

                        if (currNode.Attributes["default"] != null)
                            value = currNode.Attributes["default"].Value;

                        if (currNode.Attributes["type"] != null)
                            type = currNode.Attributes["type"].Value;

                        //try to load via sub nodes
                        
                        foreach (XmlNode subNode in currNode.ChildNodes) {
                            if (subNode.Name == "id")
                                idString = subNode.InnerText;

                            if (subNode.Name == "name")
                                name = subNode.InnerText;

                            if (subNode.Name == "description")
                                description = subNode.InnerText;

                            if (subNode.Name == "default")
                                value = subNode.InnerText;

                            if (subNode.Name == "type")
                                type = subNode.InnerText;
                        }

                        if (idString == "" || name == "" || type == "") 
                            throw new Exception("Critical fields for DBSetting object not specified");
                        
                        // try to add the setting
                        bool wasNewSetting;
                        wasNewSetting = AddSetting(idString, parentGroups, name, description, value, type);

                        if (wasNewSetting)
                            newSettingCount++;
                    }
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(ThreadAbortException))
                        throw e;

                    string settingName = "???";
                    if (currNode != null && currNode.Attributes != null && currNode.Attributes["name"] != null)
                        settingName = currNode.Attributes["name"].Value;

                    logger.Error("Internal Error loading initial setting \"" + settingName + "\"");
                }
            }

            if (newSettingCount > 0)
                logger.Info("Added " + newSettingCount + " new setting(s) to the database.");
        }

        #endregion

        public SettingsManager(DatabaseManager DBManager) {
            this.DBManager = DBManager;
            List<DBSetting> settingList = DBManager.Get<DBSetting>(null);
            foreach (DBSetting currSetting in settingList) 
                this.Add(currSetting.Key, currSetting);

            logger.Info("SettingsManager Created");
        }

        ~SettingsManager() {
            Shutdown();
        }

        public void Shutdown() {
            if (DBManager != null) {
                logger.Info("SettingsManager Shutting Down");
                commit();
                DBManager = null;
            }
        }

        public void LoadSettingsFile(string settingsXML, bool overwriteExistingSettings) {
            // attempts to convert the provided string into an XmlDocument
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(settingsXML);
            
            if (xml.DocumentElement.Name != "MoviesPluginSettings") {
                logger.Error("Error parsing settings file.");
                return;
            }

            processChildNodes(xml.DocumentElement, null);

        }

        public bool AddSetting(string key, List<string> groups, String name, String description, String value, String type) {

            if (!this.ContainsKey(key)) {
                DBSetting newSetting = new DBSetting();
                newSetting.Key = key;
                newSetting.Name = name;
                newSetting.Value = value;
                newSetting.Type = type;
                newSetting.Description = description;
                newSetting.Grouping.AddRange(groups);

                this.Add(key, newSetting);
                return true;
            } else {
                DBSetting existingSetting = this[key];
                
                // update name if neccisary
                if (!existingSetting.Name.Equals(name))
                    existingSetting.Name = name;

                // update description if neccisary
                if (!existingSetting.Description.Equals(description))
                    existingSetting.Description = description;
                
                // update groups if neccisary
                for(int i = 0; i < existingSetting.Grouping.Count; i++) {
                    if (i >= groups.Count || !existingSetting.Grouping[i].Equals(groups[i])) {
                        existingSetting.Grouping.Clear();
                        existingSetting.Grouping.AddRange(groups);
                        break;
                    }
                }

                return false;
            }
        }
    }
}
