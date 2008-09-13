using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("action")]
    public class ActionNode: ScraperNode {
        #region Properties

        public override Dictionary<string, string> InputVariables {
            get {
                return base.InputVariables;
            }
            set {
                // for the action node we want to create a new global variable
                // object and we dont want to modify the dictionary passed in
                // by the calling class, so we override here.
                inputVariables.Clear();
                globalVariables.Clear();
                foreach (KeyValuePair<string, string> currPair in value) {
                    inputVariables[currPair.Key] = currPair.Value;
                    globalVariables[currPair.Key] = currPair.Value;
                }
            }
        }

        #endregion

        #region Methods

        public ActionNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {
        
            
        }

        public override void Execute() {
            base.Execute();
            executeChildren();
        }

        #endregion

    }
}
