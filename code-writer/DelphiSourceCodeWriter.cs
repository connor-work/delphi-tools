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

using System;
using System.Collections.Generic;
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
        /// Constructs the recommended source file path for a unit.
        /// </summary>
        /// <param name="unit">The unit</param>
        /// <returns>Sequence of path components for the recommended source file</returns>
        public static IEnumerable<string> ToSourceFilePath(this Unit unit) => unit.Heading.Namespace.Append($"{unit.Heading.Unit}.pas");

        /// <summary>
        /// Constructs Delphi source code defining a unit.
        /// </summary>
        /// <param name="unit">The unit to define</param>
        /// <returns>The Delphi source code</returns>
        public static string ToSourceCode(this Unit unit) => new DelphiSourceCodeWriter().Append(unit).ToString();
    }

    /// <summary>
    /// Extensions to <see cref="string"/> for source code production.
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// Recognized separators for syllables in an identifier string.
        /// </summary>
        private static readonly string[] syllableSeparators = new string[] { "-", "_" };

        /// <summary>
        /// Converts an identifier string to pascal case, removing syllable separators and capitalizing the first letter of each syllable.
        /// </summary>
        /// <param name="identifier">The identifier</param>
        /// <returns>Pascal-case equivalent identifier</returns>
        public static string ToPascalCase(this string identifier) => string.Concat(identifier.Split(syllableSeparators, StringSplitOptions.RemoveEmptyEntries)
                                                                                             .Select(syllable => syllable.First().ToString().ToUpper() + syllable.Substring(1)));
    }

    /// <summary>
    /// Tool for the programmatic production of Delphi language source code.
    /// Usage is similar to <see cref="StringBuilder"/>. The resulting string is a fragment of a Delphi source code file.
    /// </summary>
    public class DelphiSourceCodeWriter
    {
        /// <summary>
        /// Default file name extension (without leading dot) for Delphi unit source files
        /// </summary>
        public static readonly string unitSourceFileExtension = "pas";

        /// <summary>
        /// Line separator for Delphi source code
        /// </summary>
        private static readonly string lineSeparator = "\n";

        /// <summary>
        /// String used to indent source code lines by one level
        /// </summary>
        private static readonly string singleIndent = new String(' ', 2);

        /// <summary>
        /// Constructs a prefix string for indenting source code lines.
        /// </summary>
        /// <param name="level">The indentation level</param>
        /// <returns>Resulting prefix string</returns>
        private static string Indent(int level) => string.Concat(Enumerable.Repeat(singleIndent, level));

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
        /// Appends an arbitrary string to the source code, performing line separator conversion.
        /// </summary>
        /// <param name="text">The string to append</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter Append(string text)
        {
            codeBuilder.Append(Regex.Replace(text, @"\r\n?|\n", lineSeparator));
            return this;
        }

        /// <summary>
        /// Appends the default line terminator.
        /// </summary>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter AppendLine()
        {
            codeBuilder.Append(lineSeparator);
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

#pragma warning disable S4136 // Method overloads should be grouped together -> "Append* method order reflects order in protobuf schema here

        /// <summary>
        /// Appends Delphi source code defining a unit.
        /// </summary>
        /// <param name="unit">The unit to define</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Unit unit) => AppendDelphiCode(
$@"unit {unit.Heading.ToSourceCode()};
"
            )
            .Append(unit.Interface)
            .Append(unit.Implementation)
            .AppendDelphiCode(
$@"
end.
"
            );

        /// <summary>
        /// Appends Delphi source code for an interface section of a unit.
        /// </summary>
        /// <param name="interface">The interface section</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Interface @interface)
        {
            AppendDelphiCode(
$@"
interface
"
            ).AppendUsesClause(@interface.UsesClause);
            foreach (InterfaceDeclaration declaration in @interface.Declarations) Append(declaration);
            return this;
        }

        /// <summary>
        /// Appends Delphi source code for a uses clause.
        /// </summary>
        /// <param name="interface">List of unit references in the uses clause</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter AppendUsesClause(IList<UnitReference> references)
        {
            // No clause required without references
            if (references.Count == 0) return this;
            AppendDelphiCode(
 $@"
uses"
            );
            bool first = true;
            foreach (UnitReference reference in references)
            {
                if (!first) Append(",");
                first = false;
                AppendDelphiCode(
$@"
{reference.Unit.ToSourceCode()}"
                , 1);
            }
            return Append(";").AppendLine();
        }

        /// <summary>
        /// Appends Delphi source code for an implementation section of a unit.
        /// </summary>
        /// <param name="interface">The implementation section</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Implementation implementation) => AppendDelphiCode(
 $@"
implementation
"
            ).AppendUsesClause(implementation.UsesClause);

        /// <summary>
        /// Appends Delphi source code for a declaration that appears in an interface section.
        /// </summary>
        /// <param name="declaration">The declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(InterfaceDeclaration declaration) => declaration.DeclarationCase switch
        {
            InterfaceDeclaration.DeclarationOneofCase.ClassDeclaration => Append(declaration.ClassDeclaration),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for a class declaration.
        /// </summary>
        /// <param name="class">The class declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ClassDeclaration @class)
        {
            AppendDelphiCode(
$@"
type
"
            );
            AppendDelphiCode(
$@"{@class.Name} = class
end;
"
            , 1);
            return this;
        }
    }
}

#pragma warning restore S4136 // Method overloads should be grouped together
