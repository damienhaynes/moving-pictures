using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Cornerstone.ScraperEngine.Nodes {
    [ScraperNode("action")]
    public class ActionNode: ScraperNode {
        #region Properties

        public override string Name {
            get { return name; }
            set { name = value; }
        } protected string name;

        public override string Input {
            get { return null; }
            set { }  
        }

        public override string InputLabel {
            get { return null; }
        }

        public override string Output {
            get { return null; }
        }

        public override string OutputLabel {
            get { return null; }
        }

        #endregion

        #region Methods

        public ActionNode(XmlNode xmlNode)
            : base(xmlNode) {
        
            
        }
        
        #endregion

    }
}
