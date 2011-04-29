using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using NLog;

namespace Cornerstone.MP {
    public abstract class SkinSettings {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected Dictionary<string, string> defines;

        public SkinSettings(string skinFileName) {
            loadDefinesFromSkin(skinFileName);
            populateProperties();
        }

        // Grabs the <define> tags from the skin for skin parameters from skinner.
        public void loadDefinesFromSkin(string skinFileName) {
            try {
                // Load the XML file
                XmlDocument doc = new XmlDocument();
                logger.Info("Loading defines from skin.");
                doc.Load(skinFileName);

                // parse out the define tags and store them
                defines = new Dictionary<string, string>();
                foreach (XmlNode node in doc.SelectNodes("/window/define")) {
                    string[] tokens = node.InnerText.Split(':');

                    if (tokens.Length < 2)
                        continue;

                    defines[tokens[0]] = tokens[1];
                    logger.Debug("Loaded define from skin: " + tokens[0] + ": " + tokens[1]);
                }

                
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error loading <define> tags from skin file.", e);
            }
        }

        private void populateProperties() {
            // loop through our properties and for all defined as a skinsetting, try to load the value
            foreach (PropertyInfo currProperty in GetType().GetProperties()) {
                try {
                    // try to grab the attribute for this proeprty, if it doesnt exist,
                    // this isnt a skin setting
                    object[] attributeList = currProperty.GetCustomAttributes(typeof(SkinSettingAttribute), true);
                    if (attributeList.Length == 0) 
                        continue;
                    
                    // grab the setting attribute and set use the default value for now
                    SkinSettingAttribute skinSettingAttr = (SkinSettingAttribute)attributeList[0];
                    object value = skinSettingAttr.Default;

                    // try to grab the skin defined value
                    if (defines.ContainsKey(skinSettingAttr.SettingName)) {
                        string stringValue = defines[skinSettingAttr.SettingName];
                        
                        // try parsing as a int
                        if (currProperty.PropertyType == typeof(int)) {
                            int intValue;
                            if (int.TryParse(stringValue, out intValue)) 
                                value = intValue;
                            else
                                logger.Error("\"" + stringValue + "\" is an invalid value for " + skinSettingAttr.SettingName + " skin setting (expecting an int). Using default value.");
                        }
                        
                        // try parsing as a float
                        else if (currProperty.PropertyType == typeof(float)) {
                            float floatValue;
                            if (float.TryParse(stringValue, out floatValue))
                                value = floatValue;
                            else
                                logger.Error("\"" + stringValue + "\" is an invalid value for " + skinSettingAttr.SettingName + " skin setting (expecting a float). Using default value.");
                        }

                        // try parsing as a double
                        else if (currProperty.PropertyType == typeof(double)) {
                            double doubleValue;
                            if (double.TryParse(stringValue, out doubleValue))
                                value = doubleValue;
                            else
                                logger.Error("\"" + stringValue + "\" is an invalid value for " + skinSettingAttr.SettingName + " skin setting (expecting a double). Using default value.");
                        }

                        // try parsing as a bool
                        else if (currProperty.PropertyType == typeof(bool)) {
                            bool boolValue;
                            if (bool.TryParse(stringValue, out boolValue))
                                value = boolValue;
                            else    
                                logger.Error("\"" + stringValue + "\" is an invalid value for " + skinSettingAttr.SettingName + " skin setting (expecting true or false). Using default value.");
                        }

                        // try parsing as a char
                        else if (currProperty.PropertyType == typeof(char)) {
                            if (stringValue.Length > 0)
                                logger.Error("\"" + stringValue + "\" is an invalid value for " + skinSettingAttr.SettingName + " skin setting (expecting a single character). Using default value.");

                            value = stringValue[0];
                        }

                        // try parsing as a string
                        else if (currProperty.PropertyType == typeof(string)) {
                            value = stringValue;    
                        }
                        
                        // unsupported skin setting type
                        else {
                            logger.Error(currProperty.PropertyType.Name + " is not a supported SkinSetting type.");
                        }

                    } else {
                        logger.Debug(skinSettingAttr.SettingName + " not defined in skin file, using default.");
                    }

                    // try to assign the value to the property
                    try {
                        currProperty.SetValue(this, value, null);
                        logger.Info("Assigned skin setting: " + currProperty.Name + " (" + skinSettingAttr.SettingName + ") = " + value.ToString());
                    }
                    catch (Exception) {
                        logger.Error("Failed to assign SkinSetting " + currProperty.Name + " (" + skinSettingAttr.SettingName + ")");
                    }

                }
                catch (Exception e) {
                    logger.ErrorException("Unexpected error processing skin settings!", e);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SkinSettingAttribute : Attribute {
        public string SettingName {
            get { return _settingName; }
            set { _settingName = value; }
        }
        private string _settingName;

        public object Default {
            get { return _default; }
            set { _default = value; }
        }
        private object _default;

        public SkinSettingAttribute(string settingName, object defaultValue) {
            _settingName = settingName;
            _default = defaultValue;
        }
    }
}
