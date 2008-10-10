using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("set")]
    public class SetNode: ScraperNode {
        #region Properties

        public string Value {
            get { return value; }
        } protected String value;

        #endregion

        #region Methods

        public SetNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {

            if (DebugMode) logger.Debug("executing set: " + xmlNode.OuterXml);
            
            // try to grab the value
            try { value = xmlNode.Attributes["value"].Value; }
            catch (Exception) {
              loadSuccess = false;
            }
            
            // get the innervalue
            string innerValue = xmlNode.InnerText.Trim();
            
            // Display an error if two values are set 
            if (loadSuccess && !innerValue.Equals(String.Empty)) {
              logger.Error("Ambiguous assignment on: " + xmlNode.OuterXml);
              loadSuccess = false;
              return;
            }

            // Display an error if no values are set
            if (!loadSuccess && innerValue.Equals(String.Empty)) {
              logger.Error("Missing VALUE attribute on: " + xmlNode.OuterXml);
              return;
            }

            // Use the innerValue if we don't have a VALUE attribute
            if (!loadSuccess && !innerValue.Equals(String.Empty)) {
              loadSuccess = true;
              value = innerValue;              
            }

        }

        public override void Execute(Dictionary<string, string> variables) {
            setVariable(variables, parseString(variables, Name), parseString(variables, value));
        }

        #endregion
    }
}
