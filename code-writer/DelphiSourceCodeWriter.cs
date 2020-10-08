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
    /// Extensions similar to <see cref="System.Linq"/>, used for functional operations
    /// </summary>
    public static partial class LinqExtensions
    {
        /// <summary>
        /// Performs a partial application of a function for each element of a sequence.
        /// </summary>
        /// <typeparam name="T">Type of the sequence elements (function parameters)</typeparam>
        /// <param name="parameters">Sequence of function parameters</param>
        /// <param name="action">Function whose first parameter accepts a sequence element</param>
        /// <returns>Sequence of partially applied functions</returns>
        public static IEnumerable<Action> PartiallyApply<T>(this IEnumerable<T> parameters, Action<T> action) => parameters.Select(parameter => (Action)(() => action.Invoke(parameter)));
    }

    /// <summary>
    /// Extensions to Delphi source code types for source code production.
    /// </summary>
    public static partial class SourceCodeExtensions
    {
        /// <summary>
        /// Constructs the recommended source file path for a unit.
        /// </summary>
        /// <param name="unit">The unit</param>
        /// <returns>Sequence of path components for the recommended source file</returns>
        public static IEnumerable<string> ToSourceFilePath(this Unit unit) => unit.Heading.Namespace.Append($"{unit.Heading.ToSourceCode()}.pas");

#pragma warning disable S4136 // Method overloads should be grouped together -> "ToSourceCode* method order reflects order in protobuf schema here

        /// <summary>
        /// Constructs Delphi source code defining a unit.
        /// </summary>
        /// <param name="unit">The unit to define</param>
        /// <returns>The Delphi source code</returns>
        public static string ToSourceCode(this Unit unit) => new DelphiSourceCodeWriter().Append(unit).ToString();

        /// <summary>
        /// Constructs a Delphi identifier string for a Delphi unit identifier.
        /// </summary>
        /// <param name="name">The unit identifier, either a generic name or fully qualified name of the unit</param>
        /// <returns>The Delphi identifier string</returns>
        public static string ToSourceCode(this UnitIdentifier identifier) => string.Join(".", identifier.Namespace.Append(identifier.Unit));

        /// <summary>
        /// Constructs an optional Delphi visibility specifier string for declaring the visibility attribute of a Delphi class member.
        /// </summary>
        /// <param name="visibility">The visibility specifier</param>
        /// <returns>The Delphi visibility specifier string, if one is required</returns>
        internal static string? ToSourceCode(this Visibility visibility) => visibility switch
        {
            Visibility.Unspecified => null,
            Visibility.Private => "private",
            Visibility.Protected => "protected",
            Visibility.Public => "public",
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Constructs a Delphi source code string that is prepended to the declaration of a Delphi class member to specify its visibility attribute.
        /// </summary>
        /// <param name="visibility">The visibility specifier</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToDeclarationPrefix(this Visibility visibility)
        {
            string? visibilitySpecifier = visibility.ToSourceCode();
            return visibilitySpecifier == null ? "" : $"{visibilitySpecifier} ";
        }

        /// <summary>
        /// Constructs a Delphi source code string for a Delphi prototype of a procedure.
        /// </summary>
        /// <param name="prototype">The prototype</param>
        /// <param name="@class">Optional name of the containing class, if the prototype is part of a method's defining declaration</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToSourceCode(this Prototype prototype, string? @class = null)
        {
            string classPrefix = @class == null ? "" : $"{@class}.";
            string parameterSuffix = "";
            if (prototype.ParameterList.Count != 0) parameterSuffix = $"({string.Join("; ", prototype.ParameterList.Select(parameter => parameter.ToSourceCode()))})";
            string returnTypeSuffix = "";
            if (prototype.Type == Prototype.Types.Type.Function) returnTypeSuffix = $": {prototype.ReturnType}";
            return $"{prototype.Type.ToSourceCode()} {classPrefix}{prototype.Name}{parameterSuffix}{returnTypeSuffix}";
        }

        /// <summary>
        /// Constructs a Delphi keyword string for declaring the specific type of a Delphi procedure prototype.
        /// </summary>
        /// <param name="type">The prototype type</param>
        /// <returns>The Delphi keyword string</returns>
        internal static string ToSourceCode(this Prototype.Types.Type type) => type switch
        {
            Prototype.Types.Type.Procedure => "procedure",
            Prototype.Types.Type.Constructor => "constructor",
            Prototype.Types.Type.Destructor => "destructor",
            Prototype.Types.Type.Function => "function",
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Constructs an optional Delphi directive string for declaring the binding of a Delphi method.
        /// </summary>
        /// <param name="binding">The type of method binding</param>
        /// <returns>The Delphi directive string, if one is required</returns>
        internal static string? ToSourceCode(this MethodInterfaceDeclaration.Types.Binding binding) => binding switch
        {
            MethodInterfaceDeclaration.Types.Binding.Static => null,
            MethodInterfaceDeclaration.Types.Binding.Virtual => "virtual",
            MethodInterfaceDeclaration.Types.Binding.Override => "override",
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Constructs a Delphi source code string that is appended to the interface declaration of a Delphi method to specify its binding.
        /// </summary>
        ///  <param name="binding">The type of method binding</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToDeclarationSuffix(this MethodInterfaceDeclaration.Types.Binding binding)
        {
            string? directive = binding.ToSourceCode();
            return directive == null ? "" : $" {directive};";
        }

        /// <summary>
        /// Constructs a Delphi source code string for a parameter declaration in a parameter list.
        /// </summary>
        /// <param name="parameter">The parameter declaration</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToSourceCode(this Parameter parameter) => $"{parameter.Name}: {parameter.Type}";

        /// <summary>
        /// Constructs a Delphi source code string for a Delphi field declaration.
        /// </summary>
        /// <param name="field">The field declaration</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToSourceCode(this FieldDeclaration field) => $"var {field.Name}: {field.Type}";

        /// <summary>
        /// Constructs a Delphi source code string for a Delphi property declaration.
        /// </summary>
        /// <param name="property">The property declaration</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToSourceCode(this PropertyDeclaration property)
        {
            string readSpecifierSuffix = property.ReadSpecifier.Length == 0 ? "" : $" read {property.ReadSpecifier}";
            string writeSpecifierSuffix = property.WriteSpecifier.Length == 0 ? "" : $" write {property.WriteSpecifier}";
            return $"property {property.Name}: {property.Type}{readSpecifierSuffix}{writeSpecifierSuffix}";
        }

        /// <summary>
        /// Constructs a Delphi source code string for a Delphi declaration of an enumerated value.
        /// </summary>
        /// <param name="value">The declaration of the enumerated value</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToSourceCode(this EnumValueDeclaration value)
        {
            string ordinalitySuffix = value.OptionalOrdinalityCase switch
            {
                EnumValueDeclaration.OptionalOrdinalityOneofCase.Ordinality => $" = {value.Ordinality}",
                EnumValueDeclaration.OptionalOrdinalityOneofCase.None => "",
                _ => throw new NotImplementedException()
            };
            return $"{value.Name}{ordinalitySuffix}";
        }

        /// <summary>
        /// Constructs Delphi source code defining a program.
        /// </summary>
        /// <param name="program">The program to define</param>
        /// <returns>The Delphi source code</returns>
        public static string ToSourceCode(this Program program) => new DelphiSourceCodeWriter().Append(program).ToString();

#pragma warning restore S4136 // Method overloads should be grouped together

    }

    /// <summary>
    /// Extensions to <see cref="string"/> for source code production.
    /// </summary>
    public static partial class StringExtensions
    {
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
        /// Converts a human-readable identifier string to pascal case, splitting it into "syllables" and capitalizing the first letter of each syllable.
        /// This case style is commonly used for Delphi identifiers that do not refer to constants (before addition of a prefix).
        /// </summary>
        /// <param name="identifier">The identifier</param>
        /// <returns>Pascal-case equivalent identifier</returns>
        public static string ToPascalCase(this string identifier) => string.Concat(identifier.SplitSyllables()
                                                                                             .Select(syllable => syllable.First().ToString().ToUpper() + syllable.Substring(1).ToLower()));

        /// <summary>
        /// Converts a human-readable string to screaming snake case, splitting it into "syllables", capitalizing all letters and joining syllables with an underscore.
        /// This case style is commonly used for Delphi identifiers that refer to constants.
        /// </summary>
        /// <param name="identifier">The identifier</param>
        /// <returns>Pascal-case equivalent identifier</returns>
        public static string ToScreamingSnakeCase(this string identifier) => string.Join("_", identifier.SplitSyllables().Select(syllable => syllable.ToUpper()));

        /// <summary>
        /// Converts all line separators in a string to the same form.
        /// </summary>
        /// <param name="text">The original string</param>
        /// <param name="lineSeparator">The new line separator</param>
        /// <returns>The new string</returns>
        internal static string ConvertLineSeparators(this string text, string lineSeparator) => Regex.Replace(text, @"\r\n?|\n", lineSeparator);
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
        /// Default file name extension (without leading dot) for Delphi program source files
        /// </summary>
        public static readonly string programSourceFileExtension = "pas";

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
        private static string IndentPrefix(int level) => string.Concat(Enumerable.Repeat(singleIndent, level));

        /// <summary>
        /// Currently produced Delphi source code
        /// </summary>
        private readonly StringBuilder codeBuilder = new StringBuilder();

        /// <summary>
        /// Current indentation level
        /// </summary>
        private int indentLevel = 0;

        /// <summary>
        /// Converts the already produced Delphi source code to a string.
        /// </summary>
        /// <returns>Delphi source code string</returns>
        [Pure]
        public override string ToString() => codeBuilder.ToString();

        /// <summary>
        /// Changes the base identation level for the Delphi source code appended afterwards.
        /// </summary>
        /// <param name="shift">Change to the identation level</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Indent(int shift)
        {
            indentLevel += shift;
            return this;
        }

        /// <summary>
        /// Appends an arbitrary string to the source code, performing line separator conversion.
        /// </summary>
        /// <param name="text">The string to append</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter Append(string text)
        {
            codeBuilder.Append(text.ConvertLineSeparators(lineSeparator));
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
        /// Performs multiple append operations, inserting a blank line as padding using <see cref="AppendLine"/> between consecutive operations.
        /// </summary>
        /// <param name="appends">Sequence of append operations to be performed</param>
        /// <param name="padEnd">If <see langword="true"/>, a non-empty sequence is also padded after the last element</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter AppendMultiplePadded(IEnumerable<Action> appends, bool padEnd = false)
        {
            bool firstLine = true;
            foreach (Action append in appends)
            {
                if (!firstLine) AppendLine();
                firstLine = false;
                append.Invoke();
            }
            if (!firstLine && padEnd) AppendLine();
            return this;
        }

        /// <summary>
        /// Appends arbitrary Delphi source code and applies indentation.
        /// </summary>
        /// <param name="lines">Delphi source code string, potentially multi-line</param>
        /// <param name="shift">Optional change to the indentation level applied only to this source code string</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter AppendDelphiCode(string lines, int shift = 0)
        {
            Indent(shift);
            codeBuilder.Append(Regex.Replace(lines.ConvertLineSeparators(lineSeparator),
                                             "^.+$" /* any non-empty line */,
                                             IndentPrefix(indentLevel) + "$&" /* is prefixed with indentation */,
                                             RegexOptions.Multiline));
            Indent(-shift);
            return this;
        }

#pragma warning disable S4136 // Method overloads should be grouped together -> "Append* method order reflects order in protobuf schema here

        /// <summary>
        /// Appends Delphi source code defining a unit.
        /// </summary>
        /// <param name="unit">The unit to define</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Unit unit)
        {
            if (unit.Comment != null) Append(unit.Comment);
            return AppendDelphiCode(
$@"unit {unit.Heading.ToSourceCode()};

{{$IFDEF FPC}}
  {{$MODE DELPHI}}
{{$ENDIF}}

"
            ).Append(unit.Interface)
            .Append(unit.Implementation)
            .AppendDelphiCode(
$@"end.
"
            );
        }

        /// <summary>
        /// Appends Delphi source code for an interface section of a unit.
        /// </summary>
        /// <param name="interface">The interface section</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Interface @interface) => AppendDelphiCode(
$@"interface

"
            ).AppendUsesClause(@interface.UsesClause)
            .AppendMultiplePadded(@interface.Declarations.PartiallyApply(declaration => Append(declaration)),
                                   true);

        /// <summary>
        /// Appends Delphi source code for a uses clause.
        /// </summary>
        /// <param name="references">List of unit references in the uses clause</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter AppendUsesClause(IList<UnitReference> references)
        {
            // No clause required without references
            if (references.Count == 0) return this;
            AppendDelphiCode(
 $@"uses
"
            );
            bool first = true;
            foreach (UnitReference reference in references)
            {
                if (!first) Append(",").AppendLine();
                first = false;
                AppendDelphiCode(
$@"{reference.Unit.ToSourceCode()}"
                , 1);
            }
            return Append(";").AppendLine().AppendLine();
        }

        /// <summary>
        /// Appends Delphi source code for an implementation section of a unit.
        /// </summary>
        /// <param name="implementation">The implementation section</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Implementation implementation) => AppendDelphiCode(
$@"implementation

"
            ).AppendUsesClause(implementation.UsesClause)
            .AppendMultiplePadded(implementation.Declarations.PartiallyApply(declaration => Append(declaration)),
                                  true);

        /// <summary>
        /// Appends Delphi source code for a declaration that appears in an interface section.
        /// </summary>
        /// <param name="declaration">The declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(InterfaceDeclaration declaration) => declaration.DeclarationCase switch
        {
            InterfaceDeclaration.DeclarationOneofCase.ClassDeclaration => Append(declaration.ClassDeclaration),
            InterfaceDeclaration.DeclarationOneofCase.EnumDeclaration => Append(declaration.EnumDeclaration),
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
$@"type
"
            );
            Indent(1);
            if (@class.Comment != null) Append(@class.Comment);
            string ancestorSpecifier = @class.Ancestor.Length != 0 ? $"({@class.Ancestor})" : "";
            return AppendDelphiCode(
$@"{@class.Name} = class{ancestorSpecifier}
"
            ).AppendMultiplePadded(@class.NestedConstDeclarations.PartiallyApply(@const => Append(@const))
                           .Concat(@class.MemberList.PartiallyApply(member => Append(member)))
                           .Concat(@class.NestedTypeDeclarations.PartiallyApply(nestedType => Append(nestedType))))
            .AppendDelphiCode(
$@"end;
"
            ).Indent(-1);
        }

        /// <summary>
        /// Appends Delphi source code for a nested type declaration.
        /// </summary>
        /// <param name="declaration">The declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(NestedTypeDeclaration declaration) => declaration.DeclarationCase switch
        {
            NestedTypeDeclaration.DeclarationOneofCase.ClassDeclaration => Append(declaration.ClassDeclaration),
            NestedTypeDeclaration.DeclarationOneofCase.EnumDeclaration => Append(declaration.EnumDeclaration),
            _ => throw new NotImplementedException()
        };        

        /// <summary>
        /// Appends Delphi source code for the declaration of a constant.
        /// </summary>
        /// <param name="@const">The constant declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ConstDeclaration @const) => @const.DeclarationCase switch
        {
            ConstDeclaration.DeclarationOneofCase.TrueConstDeclaration => Append(@const.TrueConstDeclaration),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for the declaration of a true constant.
        /// </summary>
        /// <param name="trueConst">The true constant declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(TrueConstDeclaration trueConst)
        {
            if (trueConst.Comment != null) Append(trueConst.Comment);
            return AppendDelphiCode(
$@"const {trueConst.Identifier} = {trueConst.Value};
"
            );
        }

        /// <summary>
        /// Appends Delphi source code for the declaration of a member of a class.
        /// </summary>
        /// <param name="classMember">The class member's declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ClassMemberDeclaration classMember) => classMember.DeclarationCase switch
        {
            ClassMemberDeclaration.DeclarationOneofCase.MethodDeclaration => Append(classMember.MethodDeclaration, classMember.Visibility),
            ClassMemberDeclaration.DeclarationOneofCase.FieldDeclaration => Append(classMember.FieldDeclaration, classMember.Visibility),
            ClassMemberDeclaration.DeclarationOneofCase.PropertyDeclaration => Append(classMember.PropertyDeclaration, classMember.Visibility),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for the interface declaration of a method of a class.
        /// </summary>
        /// <param name="method">The method interface declaration</param>
        /// <param name="visibility">Visibility specifier of the method</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(MethodInterfaceDeclaration method, Visibility visibility)
        {
            if (method.Comment != null) Append(method.Comment);
            return AppendDelphiCode(
$@"{visibility.ToDeclarationPrefix()}{method.Prototype.ToSourceCode()};{method.Binding.ToDeclarationSuffix()}
"
            );
        }

        /// <summary>
        /// Appends Delphi source code for the declaration of a field of a class.
        /// </summary>
        /// <param name="field">The field declaration</param>
        /// <param name="visibility">Visibility specifier of the field</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(FieldDeclaration field, Visibility visibility)
        {
            if (field.Comment != null) Append(field.Comment);
            return AppendDelphiCode(
$@"{visibility.ToDeclarationPrefix()}{field.ToSourceCode()};
"
            );
        }

        /// <summary>
        /// Appends Delphi source code for the declaration of a property of a class.
        /// </summary>
        /// <param name="property">The property declaration</param>
        /// <param name="visibility">Visibility specifier of the property</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(PropertyDeclaration property, Visibility visibility)
        {
            if (property.Comment != null) Append(property.Comment);
            return AppendDelphiCode(
$@"{visibility.ToDeclarationPrefix()}{property.ToSourceCode()};
"
            );
        }

        /// <summary>
        /// Appends Delphi source code for a declaration that appears in an implementation section.
        /// </summary>
        /// <param name="declaration">The declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ImplementationDeclaration declaration) => declaration.DeclarationCase switch
        {
            ImplementationDeclaration.DeclarationOneofCase.MethodDeclaration => Append(declaration.MethodDeclaration),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for the defining declaration of a method of a class.
        /// </summary>
        /// <param name="method">The method's defining declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(MethodDeclaration method)
        {
            AppendDelphiCode(
$@"{method.Prototype.ToSourceCode(method.Class)};
begin
"
            );
            foreach (string line in method.Statements) AppendDelphiCode(line, 1).AppendLine();
            return AppendDelphiCode(
$@"end;
"
            );
        }

        /// <summary>
        /// Appends Delphi source code for the declaration of an enumerated type.
        /// </summary>
        /// <param name="enum">The declaration of the enumerated type</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(EnumDeclaration @enum)
        {
            AppendDelphiCode(
$@"type
"
            );
            Indent(1);
            if (@enum.Comment != null) Append(@enum.Comment);
            AppendDelphiCode(
$@"{@enum.Name} = (
"
            );
            Indent(1);
            bool first = true;
            foreach (EnumValueDeclaration value in @enum.Values)
            {
                if (!first) Append(",").AppendLine().AppendLine();
                first = false;
                if (value.Comment != null) Append(value.Comment);
                AppendDelphiCode(value.ToSourceCode());
            }
            Indent(-1);
            return AppendDelphiCode(
$@"
);
"
            ).Indent(-1);
        }

        /// <summary>
        /// Appends Delphi source code for an annotation comment that describes a source code element.
        /// </summary>
        /// <param name="comment">The annotation comment</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(AnnotationComment comment) => AppendDelphiCode(string.Join("\n", comment.CommentLines.Select(line => $"/// {line}"))).AppendLine();

        /// <summary>
        /// Appends Delphi source code defining a program.
        /// </summary>
        /// <param name="program">The program to define</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(Program program)
        {
            if (program.Comment != null) Append(program.Comment);
            AppendDelphiCode(
$@"program {program.Heading};

{{$IFDEF FPC}}
  {{$MODE DELPHI}}
{{$ENDIF}}

"
            ).AppendUsesClause(program.UsesClause)
            .AppendDelphiCode(
$@"begin
"
            );
            foreach (string line in program.Block) AppendDelphiCode(line, 1).AppendLine();
            return AppendDelphiCode(
$@"end.
"
            );
        }

#pragma warning restore S4136 // Method overloads should be grouped together

    }
}
