using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Cornerstone.ScraperEngine {
    public abstract class ScraperNode {
        
        protected Dictionary<string, string> globalVariables = new Dictionary<string, string>();
        protected XmlNode xmlNode;

        #region Properties
        public string NodeType {
            get {
                foreach (Attribute currAttr in this.GetType().GetCustomAttributes(true))
                    if (currAttr is ScraperNodeAttribute)
                        return ((ScraperNodeAttribute)currAttr).NodeName;
                return null;
            }
        }

        public virtual Dictionary<string, string> InputVariables {
            get {
                return inputVariables;
            }
            set {
                inputVariables.Clear();
                foreach (KeyValuePair<string, string> currPair in value) {
                    inputVariables[currPair.Key] = currPair.Value;
                }

                globalVariables = inputVariables;
            }
        }
        protected Dictionary<string, string> inputVariables = new Dictionary<string, string>();

        public virtual Dictionary<string, string> OutputVariables {
            get { return outputVariables; }
        }
        protected Dictionary<string, string> outputVariables = new Dictionary<string, string>();
        #endregion

        #region Abstract Properties
        public abstract string Name { get; set; }

        public abstract string Input { get; set; }
        public abstract string InputLabel { get; }

        public abstract string Output { get; }
        public abstract string OutputLabel { get; }
        #endregion

        #region Methods
        
        public ScraperNode(XmlNode xmlNode) {
            this.xmlNode = xmlNode;
        }

        protected virtual void setVariable(string key, string value) {
            globalVariables[key] = value;
            outputVariables[key] = value;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ScraperNodeAttribute : System.Attribute {
        private string nodeName;

        public string NodeName {
            get { return nodeName; }
            set { nodeName = value; }
        }

        public ScraperNodeAttribute(string nodeName) {
            this.nodeName = nodeName;
        }
    }
}
