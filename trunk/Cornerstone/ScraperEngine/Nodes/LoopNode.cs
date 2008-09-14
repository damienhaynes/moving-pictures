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

        public override void Execute(Dictionary<string, string> variables) {
            string parsedName = parseString(variables, Name);
            
            int count = 0;
            while (variables.ContainsKey(loopingVariable + "[" + count + "]")) {
                string oldName = loopingVariable + "[" + count + "]";
                setVariable(variables, parsedName, parseString(variables, "${" + oldName + "}"));
                setVariable(variables, "count", count.ToString());
                transcribeArrayValues(variables, parsedName, oldName);
                
                executeChildren(variables);

                removeVariable(variables, parsedName);
                removeVariable(variables, "count");
                count++;
            }
        }

        // if the variable we are looping on itself is an array, then propogate 
        // the array elements down as well
        protected void transcribeArrayValues(Dictionary<string, string> variables, string baseName, string oldName) {
            int count = 0;
            while (variables.ContainsKey(oldName + "[" + count + "]")) {
                setVariable(variables, baseName + "[" + count + "]", parseString(variables, "${" + oldName + "[" + count + "]}"));
                transcribeArrayValues(variables, baseName + "[" + count + "]", oldName + "[" + count + "]");
                count++;
            }
        }

        #endregion
    }
}
