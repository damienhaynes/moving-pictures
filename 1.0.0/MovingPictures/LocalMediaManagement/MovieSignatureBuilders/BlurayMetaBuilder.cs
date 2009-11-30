using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.SignatureBuilders {

    /// <summary>
    /// Grabs movie title from a Bluray metadata file
    /// </summary>
    class BlurayMetaBuilder : ISignatureBuilder {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SignatureBuilderResult UpdateSignature(MovieSignature signature) {
            if (signature.LocalMedia[0].IsBluray) {
                if (signature.LocalMedia[0].File.Directory.Exists) {
                    string metaFilePath = Path.Combine(signature.LocalMedia[0].File.DirectoryName, @"META\DL\bdmt_eng.xml");
                    try {
                        XPathDocument metaXML = new XPathDocument(metaFilePath);
                        XPathNavigator navigator = metaXML.CreateNavigator();
                        XmlNamespaceManager ns = new XmlNamespaceManager(navigator.NameTable);
                        ns.AddNamespace("di", "urn:BDA:bdmv;disclib");
                        XPathNodeIterator nodes = navigator.Select("/disclib/di:discinfo/di:title/di:name", ns);
                        XPathNavigator node = nodes.Current;
                        string title = node.ToString().Trim();
                        if (title != string.Empty) {
                            signature.Title = title;
                            logger.Debug("Lookup Bluray Metafile={0}: Title= '{1}'", metaFilePath, title);
                        }
                        else {
                            logger.Debug("Lookup Bluray Metafile={0}: No Title Found", metaFilePath);
                        }
                    }
                    catch (Exception e) {
                        if (e is ThreadAbortException)
                            throw e;
                        logger.DebugException("Lookup Bluray Metafile=" + metaFilePath + ".", e);
                    }
                }
            }
            return SignatureBuilderResult.INCONCLUSIVE;
        }
    }
}
