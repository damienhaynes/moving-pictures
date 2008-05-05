using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;
using System.Windows.Forms;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables {
    [DBTableAttribute("settings")]
    public class DBSetting : MoviesPluginDBTable {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DBSetting():
            base() {
        }

        public override void CleanUpForDeletion() {
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

            set
            {
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

        public object Value {
            get {
                try {
                    if (Type == "INT")
                        return int.Parse(StringValue);
                    if (Type == "FLOAT")
                        return float.Parse(StringValue);
                    if (Type == "BOOL")
                        return bool.Parse(StringValue);
                    if (Type == "STRING")
                        return StringValue;
                }
                catch (Exception e) {
                    logger.ErrorException("Error parsing Settings Value: ", e);
                }

                return null;
            }

            set {
                StringValue = value.ToString();
            }
        }

        public bool Validate(string value) {
            try {
                if (Type == "INT") {
                    int.Parse(value);
                    return true;
                }

                if (Type == "FLOAT") {
                    float.Parse(value);
                    return true;
                }

                if (Type == "BOOL") {
                    bool.Parse(value);
                    return true;
                }

                if (Type == "STRING")
                    return true;
            }
            catch (Exception) {
                return false;
            }

            logger.Warn("Unknown Setting Type (" + Type + "), can not validate...");
            return false;
        }
    }
}
