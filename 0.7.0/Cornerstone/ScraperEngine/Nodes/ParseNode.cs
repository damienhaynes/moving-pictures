using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("parse")]
    public class ParseNode: ScraperNode {
        #region Properties

        public string Input {
            get { return input; }
        } protected String input;

        public string Pattern {
            get { return pattern; }
        } protected String pattern;

        #endregion

        #region Methods

        public ParseNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {

            // try to grab the input string
            try { input = xmlNode.Attributes["input"].Value; }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Missing INPUT attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }

            // try to grab the regex pattern
            try { pattern = xmlNode.Attributes["regex"].Value; }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Missing REGEX attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }    
        }

        public override void Execute(Dictionary<string, string> variables) {
            if (DebugMode) logger.Debug("executing parse: " + xmlNode.OuterXml);
            // parse variables from the input string
            string parsedInput = parseString(variables, input);
            string parsedName = parseString(variables, Name);
            string parsedPattern = parseString(variables, pattern);

            if (DebugMode) logger.Debug("name: " + parsedName + " ||| pattern: " + parsedPattern + " ||| input: " + parsedInput);

            // try to find matches via regex pattern
            MatchCollection matches;
            try {
                Regex regEx = new Regex(parsedPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                matches = regEx.Matches(parsedInput);
            }
            catch (Exception e) {
                logger.Error("Regex expression failed!", e);
                return;
            }

            if (matches.Count == 0) {
                if (DebugMode) logger.Debug("Parse node returned no results... " + xmlNode.OuterXml);
                return;
            }

            setVariable(variables, parsedName, matches[0].Value);

            // write matches and groups to variables
            int matchNum = 0;
            foreach (Match currMatch in matches) {
                // store the match itself
                string matchName = parsedName + "[" + matchNum + "]";
                setVariable(variables, matchName, currMatch.Value);

                // store the groups in the match
                for (int i = 1; i < currMatch.Groups.Count; i++)
                    setVariable(variables, matchName + "[" + (i - 1) + "]", currMatch.Groups[i].Value);

                matchNum++;
            }
        }

        #endregion
    }
}
