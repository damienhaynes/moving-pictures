using System;
namespace Cornerstone.ScraperEngine.Modifiers
{
    /// <summary>
    /// Interface for value modifiers
    /// </summary>
    public interface IValueModifier
    {
        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <param name="context">The context of the modifier.</param>
        /// <param name="value">The value that has to be parsed.</param>
        /// <param name="options">The options specified for the modifier.</param>
        /// <returns>the modified value</returns>
        string Parse(ScriptableScraper context, string value, string options);
    }
}
