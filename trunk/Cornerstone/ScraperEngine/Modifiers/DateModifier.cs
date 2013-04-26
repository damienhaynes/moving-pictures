using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NLog;
using Cornerstone.Extensions;

namespace Cornerstone.ScraperEngine.Modifiers
{
    /// <summary>
    /// Date adapter modifier
    /// also see:   http://msdn.microsoft.com/en-us/library/w2sa9yss.aspx
    /// format specifiers:   http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx 
    /// </summary>
    [ValueModifier("date")]
    public class DateModifier : IValueModifier
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        public string Parse(ScriptableScraper context, string value, string options)
        {
            DateTime parsedDateTime;

            // use sensible default (language of scraper)
            if (options.IsNullOrWhiteSpace())
            {
                CultureInfo ci = CultureInfo.CreateSpecificCulture(context.Language);
                if (DateTime.TryParse(value, ci, DateTimeStyles.None, out parsedDateTime))
                {
                    // store the value as the invariant datetime format
                    value = parsedDateTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
                }
                else
                {
                    logger.Error("Could not parse \"date\" modifier using script language \"" + context.Language + "\"");
                }
            }
            // use exact format as specified in options
            else
            {
                if (DateTime.TryParseExact(value, new string[] { options }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out parsedDateTime))
                {
                    // store the value as the invariant datetime format
                    value = parsedDateTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
                }
                else
                {
                    logger.Error("Could not parse \"date\" modifier using options \"" + options + "\"");
                }
            }

            return value;
        }
    }
}
