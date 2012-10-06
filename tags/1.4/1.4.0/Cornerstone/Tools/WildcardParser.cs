using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using System.Text.RegularExpressions;

namespace Cornerstone.Tools {
    public static class WildcardParser {
        public static string ParseWildcards(this DatabaseTable movie, string pattern) {
            Dictionary<string, string> mapping = getVariableMapping(movie);

            // initialize our new filename and perform regex lookup to locate variable patterns
            StringBuilder newFileName = new StringBuilder(pattern);
            Regex variableRegex = new Regex(@"\$\{(?<item>[^}]+)\}");
            MatchCollection matches = variableRegex.Matches(pattern);

            // loop through all variables and replace with actual value from movie object
            int replacementOffset = 0;
            foreach (System.Text.RegularExpressions.Match currMatch in matches) {

                // remove the current variable from the new filename
                newFileName.Remove(currMatch.Index + replacementOffset, currMatch.Length);

                // try to find a value for the variable
                string value;
                bool found = mapping.TryGetValue(currMatch.Groups["item"].Value, out value);

                // if there is no variable for what was passed move on to the next variable
                if (!found) {
                    replacementOffset -= currMatch.Length;
                    continue;
                }

                // insert value of variable that was matched and store the offset
                newFileName.Insert(currMatch.Index + replacementOffset, value);
                replacementOffset = replacementOffset - currMatch.Length + value.Length;
            }

            return newFileName.ToString();
        }

        // create the dictionary to map variables to the correct values for this movie
        public static Dictionary<string, string> getVariableMapping(DatabaseTable obj) {
            // add fields from primary object
            Dictionary<string, string> replacementStrings = new Dictionary<string, string>();
            foreach (DBField currField in DBField.GetFieldList(obj.GetType())) {
                if (currField.Filterable && currField.GetValue(obj) != null)
                    replacementStrings[currField.FieldName] = currField.GetValue(obj).ToString().Trim();
            }

            // add fields from secondary types
            foreach (DBRelation currRelation in DBRelation.GetRelations(obj.GetType())) {
                if (!currRelation.Filterable)
                    continue;

                DatabaseTable subObj = (DatabaseTable)currRelation.GetRelationList(obj)[0];

                foreach (DBField currField in DBField.GetFieldList(currRelation.SecondaryType)) {
                    if (currField.Filterable && currField.GetValue(subObj) != null)                         
                        replacementStrings[currField.FieldName] = currField.GetValue(subObj).ToString().Trim();
                }
            }

            return replacementStrings;
        }
    }
}
