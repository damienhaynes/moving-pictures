using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("sleep", LoadNameAttribute=false)]
    public class SleepNode: ScraperNode {
        #region Properties

        public int Length {
            get { return _length; }
        } protected int _length;

        #endregion

        #region Methods

        public SleepNode(XmlNode xmlNode,  ScriptableScraper context)
            : base(xmlNode, context) {

            if (context.DebugMode) logger.Debug("executing set: " + xmlNode.OuterXml);

            // Load attributes
            foreach (XmlAttribute attr in xmlNode.Attributes) {
                switch (attr.Name) {
                    case "length":
                        try { _length = int.Parse(attr.Value); }
                        catch (Exception) {
                            _length = 100;
                        }
                        break;
                }
            }

            // get the innervalue
            string innerValue = xmlNode.InnerText.Trim();

            // Validate length attribute
            if (_length <= 0) {
                logger.Error("The LENGTH attribute must be greater than 0: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }
        }

        public override void Execute(Dictionary<string, string> variables) {
            Thread.Sleep(Length);
        }

        #endregion
    }
}
