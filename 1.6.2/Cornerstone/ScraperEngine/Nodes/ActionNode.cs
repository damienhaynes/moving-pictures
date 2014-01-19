using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("action")]
    public class ActionNode: ScraperNode {


        public ActionNode(XmlNode xmlNode,  ScriptableScraper context)
            : base(xmlNode, context) {
        }

        public override void Execute(Dictionary<string, string> variables) {
            executeChildren(variables);
        }


    }
}
