using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Tools;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("if", LoadNameAttribute=false)]
    public class IfNode : ScraperNode {
        public string Test {
            get { return test; }
        } protected string test;

        public IfNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {

            // try to grab the test string
            try { test = xmlNode.Attributes["test"].Value; }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Missing TEST attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }
        }

        public override void Execute(Dictionary<string, string> variables) {
            if (DebugMode) logger.Debug("executing if: " + xmlNode.OuterXml);

            // try to split the test on the operator, quit if we fail
            Regex splitter = new Regex("\\s*(.*?)\\s*(>=|<=|!=|=|<|>)\\s*(.*)$");
            Match match = splitter.Match(test);
            if (match.Groups.Count != 4) {
                logger.Error("Error parsing test for: " + test);
                return;
            }

            string left  = match.Groups[1].Value;
            string op    = match.Groups[2].Value;
            string right = match.Groups[3].Value;
            
            left = parseString(variables, left);
            right = parseString(variables, right);

            if (DebugMode) logger.Debug("if node left value: " + left + "     right value: " + right);

            float leftNum = 0;
            float rightNum = 0;
            
            bool numeric = float.TryParse(left, out leftNum);
            numeric = numeric && float.TryParse(right, out rightNum);

            // try to process our test
            bool testPassed;
            if (op == ">=") {
                if (numeric) testPassed = leftNum >= rightNum;
                else testPassed = left.CompareTo(right) >= 0;
            } 
            else if (op == "<=") {
                if (numeric) testPassed = leftNum <= rightNum;
                else testPassed = left.CompareTo(right) <= 0;
            } 
            else if (op == "!=") {
                if (numeric) testPassed = leftNum != rightNum;
                else testPassed = left.CompareTo(right) != 0;
            } 
            else if (op == "=") {
                if (numeric) testPassed = leftNum == rightNum;
                else testPassed = left.CompareTo(right) == 0;
            } 
            else if (op == "<") {
                if (numeric) testPassed = leftNum < rightNum;
                else testPassed = left.CompareTo(right) < 0;
            } 
            else if (op == ">") {
                if (numeric) testPassed = leftNum > rightNum;
                else testPassed = left.CompareTo(right) > 0;
            }
            else {
                logger.Error("Unrecognized operator: " + op);
                return;
            }

            // if the test passed exxecute the child nodes
            if (testPassed)
                executeChildren(variables);
        }
    }
}
