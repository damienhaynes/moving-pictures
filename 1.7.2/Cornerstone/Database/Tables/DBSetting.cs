using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NLog;
using Cornerstone.Database.Tables;
using Cornerstone.Database.CustomTypes;
using System.Threading;
using System.Globalization;

namespace Cornerstone.Database.Tables {
    [DBTableAttribute("settings")]
    public class DBSetting : DatabaseTable {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DBSetting():
            base() {
        }

        public bool UpdatingFromObject {
            get { return _updatingFromObject; }
        } private bool _updatingFromObject = false;

        public bool ManagerModifyingValue {
            get { return _managerModifyingValue; }
            set { _managerModifyingValue = value; }
        } private bool _managerModifyingValue = false;
        

        public SettingsManager SettingsManager {
            get { return _settingsManager; }
            set { _settingsManager = value; }
        } private SettingsManager _settingsManager;

        public override void AfterDelete() {
        }

        #region Database Fields
        // The unique string id of the given setting.
        [DBFieldAttribute]
        public string Key {
            get { return _key; }

            set {
                _key = value;
                commitNeeded = true;
            }
        } private string _key;
        
        // The name of the given setting.
        [DBFieldAttribute]
        public string Name {
            get { return _name; }

            set {
                _name = value;
                commitNeeded = true;
            }
        } private string _name;

        // The description of the given setting.
        [DBFieldAttribute]
        public string Description {
            get { return _description; }

            set {
                _description = value;
                commitNeeded = true;
            }
        } private string _description;

        // A link to more information about this setting.
        [DBFieldAttribute]
        public string MoreInfoLink {
            get { return _moreInfoLink; }

            set {
                _moreInfoLink = value;
                commitNeeded = true;
            }
        } private string _moreInfoLink;

        [DBFieldAttribute]
        public StringList Grouping
        {

            get { return _grouping; }
            set
            {
                _grouping = value;
                commitNeeded = true;
            }

        } private StringList _grouping;


        [DBFieldAttribute(FieldName="value")]
        public string StringValue
        {
            get { return _value; }

            set {
                _value = value;
                commitNeeded = true;
            }
        } private string _value;


        // The type of data in Value. Should be INT, FLOAT, BOOL, or STRING.
        [DBFieldAttribute]
        public string Type {
            get { return _type; }

            set {
                if (value != "INT" && value != "FLOAT" && value != "BOOL" && value != "STRING")
                    return;
                _type = value;
                commitNeeded = true;
            }
        } private string _type;

        #endregion

        public bool Hidden { get; set; }
        public bool Sensitive { get; set; }

        public object Value {
            get {
                try {
                    if (Type == "INT")
                        return int.Parse(StringValue);
                    if (Type == "FLOAT")
                        return float.Parse(StringValue, new CultureInfo("en-US", false));
                    if (Type == "BOOL")
                        return bool.Parse(StringValue);
                    if (Type == "STRING")
                        return StringValue;
                }
                catch (Exception e) {
                    if (e.GetType() == typeof(ThreadAbortException))
                        throw e;
                    logger.ErrorException("Error parsing Settings Value: ", e);
                }

                return null;
            }

            set {
                if (_updatingFromObject) return;

                StringValue = value.ToString();
                _updatingFromObject = true;
                if (SettingsManager != null) SettingsManager.OnSettingChanged(_key);
                _updatingFromObject = false;

            }
        }

        public bool Validate(string value) {
            try {
                if (Type == "INT") {
                    int.Parse(value);
                    return true;
                }

                if (Type == "FLOAT") {
                    float.Parse(value, new CultureInfo("en-US", false));
                    return true;
                }

                if (Type == "BOOL") {
                    bool.Parse(value);
                    return true;
                }

                if (Type == "STRING")
                    return true;
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;
                return false;
            }

            logger.Warn("Unknown Setting Type (" + Type + "), can not validate...");
            return false;
        }

        public static string TypeLookup(Type type) {
            if (type == typeof(int)) 
                return "INT";
            else if (type == typeof(float)) 
                return "FLOAT";
            else if (type == typeof(bool)) 
                return "BOOL";
            else if (type == typeof(string)) 
                return "STRING";

            return null;
        }

        public override string ToString() {
            if (this.Sensitive)
                return "DBSetting: " + Name + " = \"*****\""; 
            else
                return "DBSetting: " + Name + " = \"" + Value.ToString() + "\""; 
        }
    }
}
