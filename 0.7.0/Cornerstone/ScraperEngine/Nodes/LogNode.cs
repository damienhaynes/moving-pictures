using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using NLog;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("log", LoadNameAttribute = false)]
    public class LogNode : ScraperNode {
        #region Properties

        public LogLevel LogLevel {
            get { return logLevel; }
        } protected LogLevel logLevel;

        public string Message {
            get { return message; }
        } protected String message;

        #endregion

        public LogNode(XmlNode xmlNode, bool debugMode)
            : base(xmlNode, debugMode) {

            try { logLevel = LogLevel.FromString(xmlNode.Attributes["LogLevel"].Value); }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logLevel = LogLevel.Debug;
            }

            try { message = xmlNode.Attributes["Message"].Value; }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException))
                    throw e;

                logger.Error("Missing MESSAGE attribute on: " + xmlNode.OuterXml);
                loadSuccess = false;
                return;
            }

            loadSuccess = true;
        }

        public override void Execute(Dictionary<string, string> variables) {
            logger.Log(this.LogLevel, parseString(variables, this.Message));
        }
    }
}
