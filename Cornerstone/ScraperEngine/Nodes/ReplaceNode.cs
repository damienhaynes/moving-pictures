using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace Cornerstone.ScraperEngine.Nodes {
  [ScraperNode("replace")]
  public class ReplaceNode : ScraperNode {
    #region Properties

    public string Input {
      get { return input; }
    } protected String input;

    public string Pattern {
      get { return pattern; }
    } protected String pattern;

    public string With {
      get { return replacement; }
    } protected String replacement;

    #endregion

    #region Methods
    public ReplaceNode(XmlNode xmlNode, bool debugMode)
      : base(xmlNode, debugMode) {

      // try to grab the input string
      try { input = xmlNode.Attributes["input"].Value; }
      catch (Exception) {
        logger.Error("Missing INPUT attribute on: " + xmlNode.OuterXml);
        loadSuccess = false;
        return;
      }

      // try to grab the regex pattern
      try { pattern = xmlNode.Attributes["pattern"].Value; }
      catch (Exception) {
        logger.Error("Missing PATTERN attribute on: " + xmlNode.OuterXml);
        loadSuccess = false;
        return;
      }

      // try to grab the regex pattern
      try { replacement = xmlNode.Attributes["with"].Value; }
      catch (Exception) {
        logger.Error("Missing WITH attribute on: " + xmlNode.OuterXml);
        loadSuccess = false;
        return;
      } 

    }

    public override void Execute(Dictionary<string, string> variables) {
      if (DebugMode) logger.Debug("executing replace: " + xmlNode.OuterXml);
      string output = string.Empty;
      try { 
        output = Regex.Replace(parseString(variables, input), parseString(variables, pattern), parseString(variables, replacement));
      }
      catch (Exception) {
        logger.Error("An error occured while executing replace.");
        return;
      } 
      setVariable(variables, parseString(variables, Name), output);
    }

    #endregion Methods
  }
}
