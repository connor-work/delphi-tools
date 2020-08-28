using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Work.Connor.Delphi.CodeWriter
{
    /// <summary>
    /// Extensions to Delphi source code types for source code production.
    /// </summary>
    public static partial class SourceCodeExtensions
    {
        /// <summary>
        /// Constructs a Delphi identifier string for a Delphi unit identifier.
        /// </summary>
        /// <param name="name">The unit identifier, either a generic name or fully qualified name of the unit</param>
        /// <returns>The Delphi identifier string</returns>
        internal static string ToSourceCode(this UnitIdentifier identifier) => string.Join(".", identifier.Namespace.Concat(new[] { identifier.Unit }));

        /// <summary>
        /// Constructs Delphi source code defining a unit.
        /// </summary>
        /// <param name="unit">The unit to define</param>
        /// <returns>The Delphi source code</returns>
        public static string ToSourceCode(this Unit unit) => new DelphiSourceCodeWriter().Append(unit).ToString();
    }

    /// <summary>
    /// Tool for the programmatic production of Delphi language source code.
    /// Usage is similar to <see cref="StringBuilder"/>. The resulting string is a fragment of a Delphi source code file.
    /// </summary>
    public class DelphiSourceCodeWriter
    {
        /// <summary>
        /// String used to indent source code lines by one level
        /// </summary>
        private static readonly string singleIndent = new String(' ', 2);

        /// <summary>
        /// Currently produced Delphi source code
        /// </summary>
        private readonly StringBuilder codeBuilder = new StringBuilder();

        /// <summary>
        /// Converts the already produced Delphi source code to a string.
        /// </summary>
        /// <returns>Delphi source code string</returns>
        [Pure]
        public override string ToString() => codeBuilder.ToString();

        /// <summary>
        /// Constructs a prefix string for indenting source code lines.
        /// </summary>
        /// <param name="level">The indentation level</param>
        /// <returns>Resulting prefix string</returns>
        private string Indent(int level) => string.Concat(Enumerable.Repeat(singleIndent, level));

        /// <summary>
        /// Appends an arbitrary string to the source code.
        /// </summary>
        /// <param name="text">The string to append</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter Append(string text)
        {
            codeBuilder.Append(text);
            return this;
        }

        /// <summary>
        /// Appends arbitrary Delphi source code and applies indentation.
        /// </summary>
        /// <param name="lines">Delphi source code string, potentially multi-line</param>
        /// <param name="level">The indentation level to shift source code lines by</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter AppendDelphiCode(string lines, int level = 0) => Append(Regex.Replace(lines,
            "^.+$" /* any non-empty line */,
            Indent(level) + "$&" /* is prefixed with indentation */,
            RegexOptions.Multiline));

        /// <summary>
        /// Appends Delphi source code defining a unit.
        /// </summary>
        /// <param name="unit">The unit to define</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Unit unit) => AppendDelphiCode(
$@"unit {unit.Heading.ToSourceCode()};
"
            )
            .AppendDelphiCode(
$@"
end.
"
            );
    }
}
