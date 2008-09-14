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

            // try to grab the value
            try { value = xmlNode.Attributes["value"].Value; }
            catch (Exception) {
                logger.Error("Missing VALUE attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }
            
        }

        public override void Execute(Dictionary<string, string> variables) {
            setVariable(variables, parseString(variables, Name), parseString(variables, value));
        }

        #endregion
    }
}
