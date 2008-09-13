using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NLog;
using System.Reflection;
using System.Web;

namespace Cornerstone.ScraperEngine {
    public abstract class ScraperNode {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, Type> nodeTypes;

        protected Dictionary<string, string> globalVariables = new Dictionary<string, string>();
        protected XmlNode xmlNode;
        protected List<ScraperNode> children;

        #region Properties
        public string NodeType {
            get {
                foreach (Attribute currAttr in this.GetType().GetCustomAttributes(true))
                    if (currAttr is ScraperNodeAttribute)
                        return ((ScraperNodeAttribute)currAttr).NodeName;
                return null;
            }
        }

        public string Name {
            get { return name; }
        } 
        protected string name;

        public string ParsedName {
            get { return parsedName; }
        }
        protected string parsedName;


        public virtual Dictionary<string, string> InputVariables {
            get {
                return inputVariables;
            }
            set {
                inputVariables.Clear();
                foreach (KeyValuePair<string, string> currPair in value) 
                    inputVariables[currPair.Key] = currPair.Value;
                

                globalVariables = value;
            }
        }
        protected Dictionary<string, string> inputVariables = new Dictionary<string, string>();

        public virtual Dictionary<string, string> OutputVariables {
            get { return outputVariables; }
        }
        protected Dictionary<string, string> outputVariables = new Dictionary<string, string>();

        public virtual bool LoadSuccess {
            get { return loadSuccess; }
        }
        protected bool loadSuccess;

        public bool DebugMode {
            get { return debugMode; }
        } private bool debugMode;

        #endregion

        #region Methods

        public ScraperNode(XmlNode xmlNode, bool debugMode) {
            this.xmlNode = xmlNode;
            children = new List<ScraperNode>();
            this.debugMode = debugMode;
            loadSuccess = loadChildren();

            // try to grab the name of the node (variable name for results)
            try { name = xmlNode.Attributes["name"].Value; }
            catch (Exception) {
                logger.Error("Missing NAME attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }

            // if it's a bad variable name we fail as well
            if (Name.Contains(" ")) {
                logger.Error("Invalid NAME attribute (no spaces allowed) \"" + Name + "\" for " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }
        }

        public virtual void Execute() {
            parsedName = parseString(name);
        }
        
        protected virtual void setVariable(string key, string value) {
            globalVariables[key] = value;
            outputVariables[key] = value;
            if (DebugMode) logger.Debug("Assigned variable: " + key + " = " + value);
        }

        protected virtual void removeVariable(string key) {
            globalVariables.Remove(key);
            outputVariables.Remove(key);
            removeArrayValues(key);
            if (DebugMode) logger.Debug("Removed variable: " + key);
        }

        private void removeArrayValues(string key) {
            int count = 0;
            while (globalVariables.ContainsKey(key + "[" + count + "]")) {
                removeVariable(key + "[" + count + "]");
                count++;
            }
        }

        protected bool loadChildren() {
            bool success = true;

            children.Clear();
            foreach (XmlNode currXmlNode in xmlNode.ChildNodes) {
                ScraperNode newScraperNode = ScraperNode.Load(currXmlNode, debugMode);
                if (newScraperNode != null && newScraperNode.loadSuccess) 
                    children.Add(newScraperNode);
                
                if (newScraperNode != null && !newScraperNode.loadSuccess)
                    success = false;
            }

            return success;
        }

        protected void executeChildren() {
            foreach (ScraperNode currChild in children) {
                currChild.InputVariables = globalVariables;
                currChild.Execute();
                foreach (KeyValuePair<string, string> currPair in currChild.OutputVariables)
                    outputVariables[currPair.Key] = currPair.Value;
            }
        }

        // scans the given string and replaces any existing variables with their value
        protected string parseString(string input) {
            StringBuilder output = new StringBuilder(input);
            foreach (KeyValuePair<string, string> currVar in globalVariables) {
                output.Replace("${" + currVar.Key + "}", currVar.Value);
                output.Replace("${" + currVar.Key + ":safe}", HttpUtility.UrlEncode(currVar.Value));
            }
            return output.ToString();
        }

        #endregion

        #region Static Methods

        static ScraperNode() {
            nodeTypes = new Dictionary<string, Type>();
        }

        public static ScraperNode Load(XmlNode xmlNode, bool debugMode) {
            if (xmlNode == null || xmlNode.NodeType == XmlNodeType.Comment)
                return null;
            
            Type nodeType = null;
            string nodeTypeName = xmlNode.Name;


            // try to grab the type from our dictionary
            if (nodeTypes.ContainsKey(nodeTypeName))
                nodeType = nodeTypes[nodeTypeName];

            // if it's not there, search the assembly for the type
            else {
                Type[] typeList = Assembly.GetExecutingAssembly().GetTypes();
                foreach (Type currType in typeList)
                    foreach (Attribute currAttr in currType.GetCustomAttributes(true))
                        if (currAttr.GetType() == typeof(ScraperNodeAttribute) &&
                            nodeTypeName.Equals(((ScraperNodeAttribute)currAttr).NodeName)) {

                            // store our type and put it in our dictionary so we dont have to
                            // look it up again
                            nodeTypes[nodeTypeName] = currType;
                            nodeType = currType;
                            break;
                        }
            }

            // if we couldn't find anything log the unhandled node and exit
            if (nodeType == null) {
                logger.Error("Unsupported node type: " + xmlNode.OuterXml);
                return null;
            }

            
            // try to create a new scraper node object
            try {
                ConstructorInfo constructor = nodeType.GetConstructor(new Type[] { typeof(XmlNode), typeof(bool) });
                ScraperNode newNode = (ScraperNode)constructor.Invoke(new object[] { xmlNode, debugMode });
                return newNode;
            }
            catch (Exception e) {
                logger.Error("Error instantiating ScraperNode based on: " + xmlNode.OuterXml, e);
                return null;
            }


        }
        
        #endregion
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ScraperNodeAttribute : System.Attribute {
        private string nodeName;

        public string NodeName {
            get { return nodeName; }
        }

        public ScraperNodeAttribute(string nodeName) {
            this.nodeName = nodeName;
        }
    }
}
