using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("loop")]
    public class LoopNode : ScraperNode{
        #region Properties

        public string LoopingVariable {
            get { return loopingVariable; }
        } protected String loopingVariable;

        #endregion

        #region Methods

        public LoopNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {

            // try to grab the looping variable
            try { loopingVariable = xmlNode.Attributes["on"].Value; }
            catch (Exception) {
                logger.Error("Missing ON attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }
        }

        public override void Execute() {
            base.Execute();

            int count = 0;
            while (globalVariables.ContainsKey(loopingVariable + "[" + count + "]")) {
                string oldName = loopingVariable + "[" + count + "]";
                setVariable(parsedName, parseString("${" + oldName + "}"));
                setVariable("count", count.ToString());
                transcribeArrayValues(parsedName, oldName);
                
                executeChildren();

                removeVariable(parsedName);
                removeVariable("count");
                count++;
            }
        }

        // if the variable we are looping on itself is an array, then propogate 
        // the array elements down as well
        protected void transcribeArrayValues(string baseName, string oldName) {
            int count = 0;
            while (globalVariables.ContainsKey(oldName + "[" + count + "]")) {
                setVariable(baseName + "[" + count + "]", parseString("${" + oldName + "[" + count + "]}"));
                transcribeArrayValues(baseName + "[" + count + "]", oldName + "[" + count + "]");
                count++;
            }
        }

        #endregion
    }
}
