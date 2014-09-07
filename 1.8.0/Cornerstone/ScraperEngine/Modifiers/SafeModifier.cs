using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NLog;

namespace Cornerstone.ScraperEngine.Modifiers
{
    [ValueModifier("safe")]
    public class SafeModifier : IValueModifier
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        public string Parse(ScriptableScraper context, string value, string options)
        {
            // if we have an encoding string try to build an encoding object
            Encoding encoding = null;
            if (options != string.Empty)
            {
                try { encoding = Encoding.GetEncoding(options.ToLower()); }
                catch (ArgumentException)
                {
                    encoding = null;
                    logger.Error("Scraper script tried to use an invalid encoding for \"safe\" modifier");
                }
            }

            if (encoding != null) {
                return HttpUtility.UrlEncode(value, encoding).Replace("+", "%20");
            }
            else {
                return HttpUtility.UrlEncode(value).Replace("+", "%20");
            }
        }
    }
}
