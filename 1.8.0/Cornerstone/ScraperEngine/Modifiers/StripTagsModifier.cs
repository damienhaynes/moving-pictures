using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NLog;

namespace Cornerstone.ScraperEngine.Modifiers
{
    [ValueModifier("striptags")]
    public class StripTagsModifier : IValueModifier
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        
        public string Parse(ScriptableScraper context, string value, string options)
        {
            value = Regex.Replace(value, @"\n", string.Empty); // Remove all linebreaks
            value = Regex.Replace(value, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase); // Replace HTML breaks with \n
            value = Regex.Replace(value, @"</p>", "\n\n", RegexOptions.IgnoreCase); // Replace paragraph tags with \n\n
            value = Regex.Replace(value, @"<.+?>", string.Empty); // Remove all other tags
            value = Regex.Replace(value, @"\n{3,}", "\n\n"); // Trim newlines
            value = Regex.Replace(value, @"\t{2,}", " ").Trim(); // Trim whitespace
            
            return HttpUtility.HtmlDecode(value);
        }
    }
}
