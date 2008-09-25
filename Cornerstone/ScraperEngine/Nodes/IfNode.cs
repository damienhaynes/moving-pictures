using System;
using System.Collections.Generic;
using System.Text;
using Cornerstone.Tools;
using System.Xml;
using System.Text.RegularExpressions;

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
            catch (Exception) {
                logger.Error("Missing TEST attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }
        }

        public override void Execute(Dictionary<string, string> variables) {
            logger.Debug("executing if: " + xmlNode.OuterXml);

            string parsedTest = parseString(variables, test);
            logger.Debug("executing if: " + parsedTest);

            // try to split the test on the operator, quit if we fail
            Regex splitter = new Regex("\\s*(.*?)\\s*(>=|<=|!=|=|<|>)\\s*(.*)$");
            Match match = splitter.Match(parsedTest);
            if (match.Groups.Count != 4) {
                logger.Error("Error parsing test for: " + parsedTest);
                return;
            }

            string left  = match.Groups[1].Value;
            string op    = match.Groups[2].Value;
            string right = match.Groups[3].Value;

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
