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
using MediaPortal.Plugins.MovingPictures.Database.CustomTypes;

namespace MediaPortal.Plugins.MovingPictures.DataProviders {
    public class ScriptableDataProvider {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string xmlScript;
        private XmlDocument xml;

        private string scriptName;
        private string language;
        private bool debug;

        private Dictionary<object, Dictionary<string, string>> savedVariables;

        public ScriptableDataProvider(string xmlScript) {
            try {
                this.xmlScript = xmlScript;
                xml = new XmlDocument();
                xml.LoadXml(xmlScript);

                if (xml.DocumentElement.Name != "ScriptableScraper") {
                    logger.Error("Error parsing scraper file.");
                    return;
                }
            }
            catch (Exception) {
                logger.Error("Error parsing script!");
                return;
            }

            try {
                scriptName = xml.DocumentElement.Attributes["name"].Value;
                language = xml.DocumentElement.Attributes["language"].Value;
                debug = bool.Parse(xml.DocumentElement.Attributes["debug"].Value);
            }
            catch (Exception) {
                logger.Error("Error loading root node attributes.");
                return;
            }

            savedVariables = new Dictionary<object, Dictionary<string, string>>();
            logger.Info("Loaded scriptable data provider: " + scriptName);
        }

        public List<Object> Execute(string action, string inputStr) {
            return Execute(action, inputStr, null);
        }


        public List<Object> Execute(string action, Object inputObj) {
            return Execute(action, string.Empty, inputObj);
        }

        public List<Object> Execute(string action, string inputStr, Object inputObj) {

            // setup the variable dictionary
            Dictionary<string, string> variables = new Dictionary<string, string>();
            if (inputStr == null) inputStr = string.Empty;
            variables["search_string"] = inputStr;

            // load saved variables if an output from a previous action
            // is now being used as an input
            if (inputObj != null && savedVariables.ContainsKey(inputObj))
                foreach (KeyValuePair<string, string> currVar in savedVariables[inputObj])
                    variables[currVar.Key] = currVar.Value;

            // grab the action node and kick off processing
            XmlNode actionNode = getActionNode(action);
            if (actionNode == null) return null;
            return processChildrenNodes(actionNode, variables, inputObj, null);

        }


        // grabs the action node for the specified action in the script
        private XmlNode getActionNode(string action) {
            foreach (XmlNode currAction in xml.DocumentElement.SelectNodes("child::action")) {
                string actionName = currAction.Attributes["name"].Value;
                if (actionName.Equals(action)) {
                    logger.Debug("Executing action: " + scriptName + ":" + actionName);
                    return currAction;
                }
            }

            logger.Error("Invalid action: " + scriptName + ":" + action);
            return null;
        }

        private List<Object> processChildrenNodes(XmlNode parentNode,
                                                        Dictionary<string, string> variables, 
                                                        Object inputObj,
                                                        Object editingObj) {
            
            List<object> rtnObjects = new List<object>();

            foreach (XmlNode currNode in parentNode.ChildNodes) {
                if (currNode.Name == "variable") 
                    processVariableNode(currNode, variables, inputObj, editingObj);
                if (currNode.Name == "retrieve") 
                    processRetrieveNode(currNode, variables, inputObj);
                if (currNode.Name == "regex")
                    rtnObjects.AddRange(processRegexNode(currNode, variables, inputObj, editingObj));
                if (currNode.Name == "object")
                    rtnObjects.AddRange(processObjectNode(currNode, variables, inputObj));
                if (currNode.Name == "field")
                    processFieldNode(currNode, variables, inputObj, editingObj);
            }

            return rtnObjects;
        }

        // processes a variable node. essentially parses for and tags and then 
        // writes the variable
        private void processVariableNode(XmlNode variableNode,
                                         Dictionary<string, string> variables,
                                         Object inputObj,
                                         Object editingObj) {
            string varName;
            string value;
                
            try {
                varName = variableNode.Attributes["name"].Value;
                value = variableNode.Attributes["value"].Value;
            }
            catch (Exception) {
                logger.Error("In " + scriptName + " scriptable data provider, missing attribute on: " + variableNode.OuterXml);
                return;
            }

            // if it's a bad variable name, ignore and move on
            if (varName.Contains(" ")) {
                logger.Error("Invalid variable name: \"" + varName + "\" in " + scriptName + " scriptable data provider.");
                return;
            }

            // parse out our variable and field tags
            value = populateVariables(value, variables);
            value = populateFields(value, inputObj);

            // store the variable
            variables[varName] = value;
            if (debug) logger.Debug("Assigned variable: " + varName + " = " + value);

            // if we have an editing object, also put variable in long term storage
            if (editingObj != null) {
                if (!savedVariables.ContainsKey(editingObj))
                    savedVariables[editingObj] = new Dictionary<string, string>();

                savedVariables[editingObj][varName] = value;
            }
        }

        // retrieves an HTML document from the specified URL
        private void processRetrieveNode(XmlNode retrieveNode, 
                                         Dictionary<string, string> variables, 
                                         Object inputObj) {

            // try to grab the name
            string varName;
            try { varName = retrieveNode.Attributes["name"].Value; }
            catch (Exception) {
                logger.Error("In " + scriptName + " scriptable data provider, missing attribute on: " + retrieveNode.OuterXml);
                return;
            }

            // if it's a bad variable name, ignore and move on
            if (varName.Contains(" ")) {
                logger.Error("Invalid variable name: \"" + varName + "\" in " + scriptName + " scriptable data provider.");
                return;
            }

            // parse out our variable and field tags
            string url = retrieveNode.Attributes["url"].Value;
            url = populateVariables(url, variables);
            url = populateFields(url, inputObj);

            // grab timeout and retry values. if none specified use defaults
            int maxRetries;
            try { maxRetries = int.Parse(retrieveNode.Attributes["retries"].Value); }
            catch (Exception){
                maxRetries = 5;
            }

            int timeout;
            try { timeout = int.Parse(retrieveNode.Attributes["timeout"].Value); }
            catch (Exception) {
                timeout = 5000;
            }

            int timeoutIncrement;
            try { timeoutIncrement = int.Parse(retrieveNode.Attributes["timeout_increment"].Value); }
            catch (Exception) {
                timeoutIncrement = 2000;
            }

            // start tryng to retrieve the document
            String pageContents = string.Empty;
            int tryCount = 0;
            while (pageContents == string.Empty) {
                try {
                    // builds the request and retrieves the respones from movie-xml.com
                    tryCount++;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout + (timeoutIncrement * tryCount);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // converts the resulting stream to a string for easier use
                    Stream resultData = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resultData, Encoding.Default, true);
                    pageContents = reader.ReadToEnd().Replace('\0', ' ');

                    resultData.Close();
                    reader.Close();
                    response.Close();

                    variables[varName] = pageContents;
                }
                catch (WebException e) {
                    if (tryCount == maxRetries) {
                        logger.ErrorException("Error connecting to URL in " + scriptName + " scriptable data provider. Reached retry limit of " + maxRetries + ". " + url, e);
                        return;
                    }
                }
            }
        }

        // executes a regex expression on the given input string. Of course 
        // previously assigned variables (perhaps from a retrieve node) can be used
        // and sub nodes will be processed iteratively, once for each match
        private List<Object> processRegexNode(XmlNode regexNode, 
                                      Dictionary<string, string> variables, 
                                      Object inputObj, Object editingObj) {

            List<object> rtnObjects = new List<object>();

            string varName;
            string pattern;
            string input;
            
            // try to grab the attributes
            try {
                varName = regexNode.Attributes["name"].Value;
                pattern = regexNode.Attributes["pattern"].Value;
                input = regexNode.Attributes["input"].Value; 
            }
            catch (Exception) {
                logger.Error("In " + scriptName + " scriptable data provider, missing attribute on: " + regexNode.OuterXml);
                return rtnObjects;
            }

            // if it's a bad variable name, ignore and move on
            if (varName.Contains(" ")) {
                logger.Error("Invalid variable name: \"" + varName + "\" in " + scriptName + " scriptable data provider.");
                return rtnObjects;
            }
            
            // parse tags from the input string
            input = populateVariables(input, variables);
            input = populateFields(input, inputObj);

            // try to find matches via regex pattern
            Regex regEx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection matches = regEx.Matches(input);

            // write match groups to variables and process child nodes for each match
            foreach (Match currMatch in matches) {
                // store the variable
                for (int i = 0; i < currMatch.Groups.Count; i++) {
                    variables[varName + ":" + i] = currMatch.Groups[i].Value;
                    if (debug) logger.Debug("Assigned variable: " + varName + ":" + i + " = " + currMatch.Groups[i].Value); 
                }

                // process child nodes here
                rtnObjects.AddRange(processChildrenNodes(regexNode, variables, inputObj, editingObj));
            }
            return rtnObjects;
        }

        // creates a new object to be returned to the application. all field tags inside this 
        // tag will be assigned to it. Also, any variable tags will be saved if the object
        // is used as an input for another action.
        private List<object> processObjectNode(XmlNode currNode, 
                                       Dictionary<string, string> variables, 
                                       Object inputObj) {

            List<object> rtnObjects = new List<object>();


            // try to grab the attributes
            string type;
            try {
                type = currNode.Attributes["type"].Value;
            }
            catch (Exception) {
                logger.Error("In " + scriptName + " scriptable data provider, missing attribute on: " + currNode.OuterXml);
                return rtnObjects;
            }

            // try to find the class
            Type[] typeList = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type currType in typeList) 
                if (currType.Name.Equals(type)) {
                    ConstructorInfo constructor = currType.GetConstructor(new Type[]{});
                    if (constructor != null) {
                        Object newObj = constructor.Invoke(null);
                        rtnObjects.Add(newObj);
                        rtnObjects.AddRange(processChildrenNodes(currNode, variables, inputObj, newObj));
                        return rtnObjects;
                    }
                }

            logger.Error("Could not find class of type \"" + type + "\" from object tag " + currNode.OuterXml);
            return rtnObjects;
        }

        private void processFieldNode(XmlNode currNode,
                                              Dictionary<string, string> variables,
                                              Object inputObj,
                                              Object editingObj) {

            Object obj;
            string fieldName;
            string value;

            // figure out what object we should be editing
            if (editingObj != null)
                obj = editingObj;
            else if (inputObj != null)
                obj = inputObj;
            else return;

            try {
                fieldName = currNode.Attributes["name"].Value;
                value = currNode.Attributes["value"].Value;
            }
            catch (Exception) {
                logger.Error("In " + scriptName + " scriptable data provider, missing attribute on: " + currNode.OuterXml);
                return;
            }

            // parse out our variable and field tags
            value = populateVariables(value, variables);
            value = populateFields(value, inputObj);
            
            // using reflection try to find the cooresponding property and write to it
            bool foundProperty = false;
            foreach (PropertyInfo currProperty in obj.GetType().GetProperties()) {
                if (currProperty.Name.Equals(fieldName)) {
                    foundProperty = true;
                    try {
                        MethodInfo set = currProperty.GetSetMethod();
                        object[] parameters = new object[1];

                        if (currProperty.PropertyType == typeof(StringList))
                            ((StringList)currProperty.GetGetMethod().Invoke(obj, null)).Add(value);
                        else {
                            if (currProperty.PropertyType == typeof(int))
                                parameters[0] = int.Parse(value);

                            if (currProperty.PropertyType == typeof(float))
                                parameters[0] = float.Parse(value);

                            if (currProperty.PropertyType == typeof(bool))
                                parameters[0] = bool.Parse(value);

                            if (currProperty.PropertyType == typeof(string))
                                parameters[0] = value;

                            set.Invoke(obj, parameters);
                        }
                    }
                    catch (Exception) {
                        logger.Error("Error assigning field to property: " + currNode.OuterXml);
                    }
                }
            }

            if (!foundProperty) {
                logger.Error("Invalid field name: \"" + fieldName + "\" in " + scriptName + " scriptable data provider.");
                return;
            }
        }

        // scans the given string and replaces any existing variables with their value
        private string populateVariables(string inputStr, Dictionary<string, string> variables) {
            StringBuilder output = new StringBuilder(inputStr);
            foreach (KeyValuePair<string, string> currVar in variables) {
                output.Replace("{var:" + currVar.Key + "}", currVar.Value);
                output.Replace("{var:" + currVar.Key + ":safe}", HttpUtility.UrlEncode(currVar.Value));
            }
            return output.ToString();
        }

        // scans through the given string and replaces and property tags with their value
        private string populateFields(string inputStr, Object inputObject) {
            if (inputObject == null)
                return inputStr;

            StringBuilder output = new StringBuilder(inputStr);
            foreach (PropertyInfo currProperty in inputObject.GetType().GetProperties()) {
                object value = currProperty.GetGetMethod().Invoke(inputObject, null);
                if (value == null)
                    continue;
                
                output.Replace("{field:" + currProperty.Name + "}", value.ToString());
                output.Replace("{field:" + currProperty.Name + ":safe}", HttpUtility.UrlEncode(value.ToString()));
            }
            return output.ToString();
        }
    }
}
