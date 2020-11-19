/// Copyright 2020 Connor Roehricht (connor.work)
/// Copyright 2020 Sotax AG
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
///     http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Work.Connor.Delphi.CodeWriter
{
    /// <summary>
    /// Extensions to <see cref="string"/> that simplify source code production.
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// Special letter case style used for identifiers in source code.
        /// </summary>
        public enum IdentifierCase
        {
            /// <summary>
            /// No case style, any valid identifier string is permitted.
            /// </summary>
            None,

            /// <summary>
            /// Pascal case, capitalizes the first letter of each syllable.
            /// </summary>
            /// <remarks>
            /// This case is commonly used for Delphi identifiers that do not refer to constants.
            /// </remarks>
            Pascal,

            /// <summary>
            /// Screaming snake case, capitalizes all letters and joins syllables with an underscore.
            /// </summary>
            /// <remarks>
            /// This case is commonly used for Delphi identifiers that refer to constants.
            /// </remarks>
            ScreamingSnake
        }

        /// <summary>
        /// Converts a human-readable identifier string to a specific case by splitting it into "syllables", and then applying case-specific rules (see <paramref name="case"/>).
        /// </summary>
        /// <param name="identifier">The identifier</param>
        /// <param name="case">The identifier case to convert to</param>
        /// <returns>Equivalent identifier with the specified case</returns>
        public static string ToCase(this string identifier, IdentifierCase @case) => @case switch
        {
            IdentifierCase.None => identifier,
            IdentifierCase.Pascal => string.Concat(identifier.SplitSyllables()
                                                  .Select(syllable => syllable.First().ToString().ToUpper() + syllable.Substring(1).ToLower())),
            IdentifierCase.ScreamingSnake => string.Join("_", identifier.SplitSyllables().Select(syllable => syllable.ToUpper())),
            _ => throw new System.NotImplementedException(),
        };

        /// <summary>
        /// Disjunction of recognized separator patterns for syllables in an identifier string
        /// </summary>
        private static readonly Regex syllableSeparator = new Regex(
            "(?:_)"                              /* a dash (variations of snake case, as in "my_name") */
            + "|" + "(?:-)"                      /* an underscore (variations of kebab case, as in "my-name") */
            + "|" + "(?:(?<=[a-z])(?=[A-Z]))"    /* boundary after a lowercase letter and before an uppercase letter (variations of Pascal case, as in "MyName") */
            + "|" + "(?:(?<=[0-9])(?=[a-zA-Z]))" /* boundary after a digit and before a letter (variations of Pascal case, as in "MyTop5Names") */
            );

        /// <summary>
        /// Splits a human-readable identifier string into "syllables", which are segments separated by any one of the patterns recognized by <see cref="syllableSeparator"/>.
        /// </summary>
        /// <param name="identifier">The identifier</param>
        /// <returns>Sequence of human-readable "syllables"</returns>
        private static IEnumerable<string> SplitSyllables(this string identifier) => syllableSeparator.Split(identifier).Where(syllable => syllable.Length != 0);

        /// <summary>
        /// Converts all line separators in a string to the same form.
        /// </summary>
        /// <param name="text">The original string</param>
        /// <param name="lineSeparator">The new line separator</param>
        /// <returns>The new string</returns>
        internal static string ConvertLineSeparators(this string text, string lineSeparator) => Regex.Replace(text, @"\r\n?|\n", lineSeparator);
    }
}
