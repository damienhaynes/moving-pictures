using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using System.Xml;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using Cornerstone.Database.CustomTypes;
using Cornerstone.ScraperEngine.Nodes;

namespace Cornerstone.ScraperEngine {
    public class ScriptableScraper {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties

        // Friendly name for the script.
        public string Name {
            get { return name; }
        } protected string name;

        // Description of the script. For display purposes.
        public string Description {
            get { return description; }
        } protected string description;

        // Description of the script. For display purposes.
        public string Author {
            get { return author; }
        } protected string author;
        
        // Friendly readable version number.
        public string Version {
            get { return versionMajor + "." + versionMinor + "." + versionPoint; }
        }

        // Major version number of script.
        public int VersionMajor {
            get { return versionMajor; }
        } protected int versionMajor;

        // Minor version number of script.
        public int VersionMinor {
            get { return versionMinor; }
        } protected int versionMinor;

        // Point version number of script.
        public int VersionPoint {
            get { return versionPoint; }
        } protected int versionPoint;

        // Unique ID number for the script.
        public int ID {
            get { return id; }
        } protected int id;

        // The type(s) of script. Used for categorization purposes. This basically defines
        // which predefined actions are implemented.
        public StringList ScriptType {
            get { return scriptType; }
        } protected StringList scriptType;

        // The language supported by the script. Used for categorization and informational 
        // purposes.
        public string Language {
            get { return language; }
        } protected string language;

        // If true, additional logging messages will be logged for this script. IMPORTANT
        // NOTE: This also requires NLog to be in Debug Mode.
        public bool DebugMode {
            get { return debug; }
            set { DebugMode = value; }
        } protected bool debug;

        // Returns true if the script loaded successfully.
        public bool LoadSuccessful {
            get { return loadSuccessful; }
        } protected bool loadSuccessful;

        public string Script {
            get { return xml.OuterXml; }
        }

        #endregion

        private XmlDocument xml;
        private Dictionary<string, ScraperNode> actionNodes;

        public ScriptableScraper(FileInfo filename) {
            // log filename here
            throw new NotImplementedException();
        }

        public ScriptableScraper(string xmlScript) {
            loadSuccessful = true;

            try {
                xml = new XmlDocument();
                xml.LoadXml(xmlScript);

                if (xml.DocumentElement.Name != "ScriptableScraper") {
                    logger.Error("Invalid root node. Expecting <ScriptableScraper>.");
                    return;
                }
            }
            catch (Exception) {
                logger.Error("Error parsing scriptable scraper XML file!");
                return;
            }

            // try to grab info from the details node
            loadDetails();
            if (!loadSuccessful)
                return;

            logger.Info("Loading scriptable scraper: " + name + " (" + id + ") Version " + Version);
            loadActionNodes();
        }

        private bool loadDetails() {
            try {
                XmlNode detailsNode = xml.DocumentElement.SelectNodes("child::details")[0];
                foreach (XmlNode currNode in detailsNode.ChildNodes) {
                    if (currNode.Name.Equals("name")) {
                        name = currNode.InnerText;
                    } else if (currNode.Name.Equals("author")) {
                        author = currNode.InnerText;
                    } else if (currNode.Name.Equals("description")) {
                        description = currNode.InnerText;
                    } else if (currNode.Name.Equals("id")) {
                        id = int.Parse(currNode.InnerText);
                    } else if (currNode.Name.Equals("version")) {
                        versionMajor = int.Parse(currNode.Attributes["major"].Value);
                        versionMinor = int.Parse(currNode.Attributes["minor"].Value);
                        versionPoint = int.Parse(currNode.Attributes["point"].Value);
                    } else if (currNode.Name.Equals("type")) {
                        scriptType = new StringList(currNode.InnerText);
                    } else if (currNode.Name.Equals("language")) {
                        language = currNode.InnerText;
                    } else if (currNode.Name.Equals("debug")) {
                        debug = bool.Parse(currNode.InnerText);
                    }
                }
            } catch (Exception) {
                logger.Info("Error parsing <details> node for scriptable scraper.");
                loadSuccessful = false;
            }

            return true;
        }

        private void loadActionNodes() {
            actionNodes = new Dictionary<string, ScraperNode>();
            foreach (XmlNode currAction in xml.DocumentElement.SelectNodes("child::action")) {
                ActionNode newNode = (ActionNode) ScraperNode.Load(currAction, false);
                if (newNode != null && newNode.LoadSuccess)
                    actionNodes[newNode.Name] = newNode;
                else {
                    logger.Error("Error loading action node: " + currAction.OuterXml);
                    loadSuccessful = false;
                }
            }
        }

        public Dictionary<string, string> Execute(string action, Dictionary<string, string> input) {
            if (actionNodes.ContainsKey(action)) {
                // transcribe the dictionary because we dont want to modify the input
                Dictionary<string, string> workingVariables = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> currPair in input)
                    workingVariables[currPair.Key] = currPair.Value;

                actionNodes[action].Execute(workingVariables);
                return workingVariables;
            }
            return null;
        }
        
    }
}
