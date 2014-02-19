using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NLog;

namespace Cornerstone.ScraperEngine.Modifiers
{
    [ValueModifier("htmldecode")]
    public class HtmlDecodeModifier : IValueModifier
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        public string Parse(ScriptableScraper context, string value, string options)
        {
            return HttpUtility.HtmlDecode(value);
        }
    }
}
