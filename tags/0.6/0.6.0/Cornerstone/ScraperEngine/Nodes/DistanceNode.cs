using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Tools;
using System.Xml;
using System.Threading;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("distance")]
    public class DistanceNode : ScraperNode {
        public string String1 {
            get { return string1; }
        } protected string string1;

        public string String2 {
            get { return string2; }
        } protected string string2;

        public DistanceNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {

            // try to grab the first value
            try { string1 = xmlNode.Attributes["string1"].Value; }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Missing STRING1 attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }

            // try to grab the second value
            try { string2 = xmlNode.Attributes["string2"].Value; }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Missing STRING2 attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }
        }

        public override void Execute(Dictionary<string, string> variables) {
            if (DebugMode) logger.Debug("executing distance: " + xmlNode.OuterXml);

            string parsedString1 = parseString(variables, string1);
            string parsedString2 = parseString(variables, string2);
            if (DebugMode) logger.Debug("executing distance: " + parsedString1 + " vs. " + parsedString2);

            int distance = AdvancedStringComparer.Levenshtein(parsedString1, parsedString2);

            setVariable(variables, parseString(variables, Name), distance.ToString());
        }

    }
}
