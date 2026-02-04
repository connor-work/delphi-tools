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
        public static readonly string programSourceFileExtension = "dpr";

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
        /// Performs multiple append operations, optionally inserting a blank line as padding using <see cref="AppendLine"/> between consecutive operations.
        /// </summary>
        /// <param name="appends">Sequence of append operations to be performed</param>
        /// <param name="padEnd">If <see langword="true"/>, a non-empty sequence is also padded after the last element</param>
        /// <param name="padBetween">If <see langword="true"/>, a blank line is inserted between consecutive operations</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter AppendMultiplePadded(IEnumerable<Action> appends, bool padEnd = false, bool padBetween = true)
        {
            bool firstLine = true;
            foreach (Action append in appends)
            {
                if (!firstLine && padBetween) AppendLine();
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
        /// <param name="absoluteIndent">If <see langword="true"/>, the indentation level is changed from 0 instead of the current level</param>
        /// <returns><c>this</c></returns>
        private DelphiSourceCodeWriter AppendDelphiCode(string lines, int shift = 0, bool absoluteIndent = false)
        {
            Indent(shift);
            codeBuilder.Append(Regex.Replace(lines.ConvertLineSeparators(lineSeparator),
                                             "^.+$" /* any non-empty line */,
                                             IndentPrefix(absoluteIndent ? shift
                                                                         : indentLevel) + "$&" /* is prefixed with indentation */,
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

"
            ).AppendMultiplePadded(unit.IncludeFiles.PartiallyApply(includeFile => AppendDelphiCode(
$@"{{$INCLUDE {EscapeIncludeFileName(includeFile)}}}
"
             )), true, false).AppendDelphiCode(
$@"{{$IFDEF FPC}}
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
        /// Escapes an include file name for embedding in a compiler directive.
        /// </summary>
        /// <param name="fileName">Name of the include file</param>
        /// <returns>The escaped name</returns>
        private string EscapeIncludeFileName(string fileName) => fileName.Contains(' ') ? $"'{fileName}'" : fileName;

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
        public DelphiSourceCodeWriter AppendUsesClause(IList<ConditionalUnitReference> references)
        {
            // No clause required without references
            if (references.Count == 0) return this;
            AppendDelphiCode(
 $@"uses
"
            );
            Action? appendReferenceAction(UnitReference? reference, bool last)
            {
                if (reference is null) return null;
                return () => AppendDelphiCode(
$@"{reference.Unit.ToSourceCode()}{(last ? ";" : ",")}
"
                , 1);
            }
            for (int i = 0; i < references.Count; i++)
            {
                bool last = i == references.Count - 1;
                ConditionalUnitReference conditionalReference = references[i];
                AppendConditionallyCompiled(conditionalReference.Condition, appendReferenceAction(conditionalReference.Element, last),
                                                                            appendReferenceAction(conditionalReference.AlternativeElement, last));
            }
            return AppendLine();
        }

        /// <summary>
        /// Appends conditionally compiled Delphi source code.
        /// </summary>
        /// <param name="condition">Optional compilation condition for the source code</param>
        /// <param name="append">Optional action appending source code that is compiled when the condition is met or absent, and suppressed otherwise</param>
        /// <param name="alternativeAppend">Optional action appending source code that is suppressed when the condition is met or absent, and compiled otherwise</param>
        /// <returns></returns>
        public DelphiSourceCodeWriter AppendConditionallyCompiled(CompilationCondition? condition, Action? append, Action? alternativeAppend)
        {
            if (condition == null) append!.Invoke();
            else
            {
                if (append != null)
                {
                    AppendDelphiCode(
$@"{{$IFDEF {condition.Symbol}}}
"
                    , 0, true);
                    append.Invoke();
                    if (alternativeAppend != null)
                    {
                        AppendDelphiCode(
$@"{{$ELSE}}
"
                        , 0, true);
                        alternativeAppend.Invoke();
                    }
                }
                else
                {
                    AppendDelphiCode(
$@"{{$IFNDEF {condition.Symbol}}}
"
                    , 0, true);
                    alternativeAppend!.Invoke();
                }
                return AppendDelphiCode(
$@"{{$ENDIF}}
"
                , 0, true);
            }
            return this;
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
            InterfaceDeclaration.DeclarationOneofCase.InterfaceTypeDeclaration => Append(declaration.InterfaceTypeDeclaration),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for a class declaration.
        /// </summary>
        /// <param name="class">The class declaration</param>
        /// <param name="visibility">Visibility specifier of the declaration, if nested in a class declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ClassDeclaration @class, Visibility visibility = Visibility.Unspecified)
        {
            AppendDelphiCode(
$@"{visibility.ToDeclarationPrefix()}type
"
            );
            Indent(1);
            if (@class.Comment != null) Append(@class.Comment);
            foreach (ConditionalAttributeAnnotation annotation in @class.AttributeAnnotations) Append(annotation);
            string inheritanceSpecifierContent = string.Join(", ", @class.Interfaces.Prepend(@class.Ancestor));
            string inheritanceSpecifier = inheritanceSpecifierContent.Length != 0 ? $"({inheritanceSpecifierContent})" : "";
            return AppendDelphiCode(
$@"{@class.Name} = class{inheritanceSpecifier}
"
            ).Indent(1)
            .AppendMultiplePadded(@class.NestedDeclarations.PartiallyApply(declaration => Append(declaration)))
            .Indent(-1)
            .AppendDelphiCode(
$@"end;
"
            ).Indent(-1);
        }

        /// <summary>
        /// Appends Delphi source code for the conditionally compiled annotation of a type or type member with an attribute.
        /// </summary>
        /// <param name="conditionalAnnotation">The annotation</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ConditionalAttributeAnnotation conditionalAnnotation)
        {
            Action? appendAnnotationAction(AttributeAnnotation? annotation)
            {
                if (annotation is null) return null;
                return () => Append(annotation);
            }
            return AppendConditionallyCompiled(conditionalAnnotation.Condition, appendAnnotationAction(conditionalAnnotation.Element),
                                                                                appendAnnotationAction(conditionalAnnotation.AlternativeElement));
        }

        /// <summary>
        /// Appends Delphi source code for the annotation of a type or type member with an attribute.
        /// </summary>
        /// <param name="annotation">The annotation</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(AttributeAnnotation annotation) => AppendDelphiCode(
$@"{annotation.ToSourceCode()}
"
            );

        /// <summary>
        /// Appends Delphi source code for a declaration nested within a class declaration.
        /// </summary>
        /// <param name="declaration">The declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ClassDeclarationNestedDeclaration declaration) => declaration.DeclarationCase switch
        {
            ClassDeclarationNestedDeclaration.DeclarationOneofCase.NestedTypeDeclaration => Append(declaration.NestedTypeDeclaration, declaration.Visibility),
            ClassDeclarationNestedDeclaration.DeclarationOneofCase.NestedConstDeclaration => Append(declaration.NestedConstDeclaration, declaration.Visibility),
            ClassDeclarationNestedDeclaration.DeclarationOneofCase.Member => Append(declaration.Member, declaration.Visibility),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for a nested type declaration.
        /// </summary>
        /// <param name="declaration">The declaration</param>
        /// <param name="visibility">Visibility specifier of the declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(NestedTypeDeclaration declaration, Visibility visibility) => declaration.DeclarationCase switch
        {
            NestedTypeDeclaration.DeclarationOneofCase.ClassDeclaration => Append(declaration.ClassDeclaration, visibility),
            NestedTypeDeclaration.DeclarationOneofCase.EnumDeclaration => Append(declaration.EnumDeclaration, visibility),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for the declaration of a constant.
        /// </summary>
        /// <param name="@const">The constant declaration</param>
        /// <param name="visibility">Visibility specifier of the declaration, if nested in a class declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ConstDeclaration @const, Visibility visibility = Visibility.Unspecified) => @const.DeclarationCase switch
        {
            ConstDeclaration.DeclarationOneofCase.TrueConstDeclaration => Append(@const.TrueConstDeclaration, visibility),
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Appends Delphi source code for the declaration of a true constant.
        /// </summary>
        /// <param name="trueConst">The true constant declaration</param>
        /// <param name="visibility">Visibility specifier of the declaration, if nested in a class declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(TrueConstDeclaration trueConst, Visibility visibility = Visibility.Unspecified)
        {
            if (trueConst.Comment != null) Append(trueConst.Comment);
            return AppendDelphiCode(
$@"{visibility.ToDeclarationPrefix()}const {trueConst.Identifier} = {trueConst.Value};
"
            );
        }

        /// <summary>
        /// Appends Delphi source code for the declaration of a member of a class.
        /// </summary>
        /// <param name="classMember">The class member's declaration</param>
        /// <param name="visibility">Visibility specifier of the member</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(ClassMemberDeclaration classMember, Visibility visibility)
        {
            return classMember.DeclarationCase switch
            {
                ClassMemberDeclaration.DeclarationOneofCase.MethodDeclaration => Append(classMember.MethodDeclaration, visibility, classMember.AttributeAnnotations),
                ClassMemberDeclaration.DeclarationOneofCase.FieldDeclaration => Append(classMember.FieldDeclaration, visibility, classMember.AttributeAnnotations),
                ClassMemberDeclaration.DeclarationOneofCase.PropertyDeclaration => Append(classMember.PropertyDeclaration, visibility, classMember.AttributeAnnotations),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Appends Delphi source code for the interface declaration of a method of a class.
        /// </summary>
        /// <param name="method">The method interface declaration</param>
        /// <param name="visibility">Visibility specifier of the method</param>
        /// <param name="annotations">Attribute annotations of the method</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(MethodInterfaceDeclaration method, Visibility visibility, IEnumerable<ConditionalAttributeAnnotation> annotations)
        {
            if (method.Comment != null) Append(method.Comment);
            foreach (ConditionalAttributeAnnotation annotation in annotations) Append(annotation);
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
        /// <param name="annotations">Attribute annotations of the field</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(FieldDeclaration field, Visibility visibility, IEnumerable<ConditionalAttributeAnnotation> annotations)
        {
            if (field.Comment != null) Append(field.Comment);
            foreach (ConditionalAttributeAnnotation annotation in annotations) Append(annotation);
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
        /// <param name="annotations">Attribute annotations of the property</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(PropertyDeclaration property, Visibility visibility, IEnumerable<ConditionalAttributeAnnotation> annotations)
        {
            if (property.Comment != null) Append(property.Comment);
            foreach (ConditionalAttributeAnnotation annotation in annotations) Append(annotation);
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
"
            );
            if (method.LocalDeclarations.Count > 0)
            {
                AppendDelphiCode(
$@"var
"
                );
                foreach (string line in method.LocalDeclarations) AppendDelphiCode(line, 1).AppendLine();
            }
            AppendDelphiCode(
$@"begin
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
        /// <param name="visibility">Visibility specifier of the declaration, if nested in a class declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(EnumDeclaration @enum, Visibility visibility = Visibility.Unspecified)
        {
            AppendDelphiCode(
$@"{visibility.ToDeclarationPrefix()}type
"
            );
            Indent(1);
            if (@enum.Comment != null) Append(@enum.Comment);
            foreach (AttributeAnnotation annotation in @enum.AttributeAnnotations) AppendDelphiCode(
$@"{annotation.ToSourceCode()}
"
            );
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
        /// Appends Delphi source code for the declaration of an interface type.
        /// </summary>
        /// <param name="interface">The declaration of the interface type</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(InterfaceTypeDeclaration @interface)
        {
            AppendDelphiCode(
$@"type
"
            );
            Indent(1);
            if (@interface.Comment != null) Append(@interface.Comment);
            foreach (ConditionalAttributeAnnotation annotation in @interface.AttributeAnnotations) Append(annotation);
            return AppendDelphiCode(
$@"{@interface.Name} = interface({@interface.Ancestor})
"
            ).Indent(1)
            .AppendDelphiCode(
$@"['{{{@interface.Guid}}}']
"
            )
            .AppendMultiplePadded(@interface.MemberDeclarations.PartiallyApply(declaration => Append(declaration)))
            .Indent(-1)
            .AppendDelphiCode(
$@"end;
"
            ).Indent(-1);
        }

        /// <summary>
        /// Appends Delphi source code for the declaration of a member of an interface.
        /// </summary>
        /// <param name="interfaceMember">The interface member's declaration</param>
        /// <returns><c>this</c></returns>
        public DelphiSourceCodeWriter Append(InterfaceMemberDeclaration interfaceMember)
        {
            return interfaceMember.DeclarationCase switch
            {
                InterfaceMemberDeclaration.DeclarationOneofCase.MethodDeclaration => Append(interfaceMember.MethodDeclaration, Visibility.Unspecified, interfaceMember.AttributeAnnotations),
                InterfaceMemberDeclaration.DeclarationOneofCase.PropertyDeclaration => Append(interfaceMember.PropertyDeclaration, Visibility.Unspecified, interfaceMember.AttributeAnnotations),
                _ => throw new NotImplementedException()
            };
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

"
            ).AppendMultiplePadded(program.IncludeFiles.PartiallyApply(includeFile => AppendDelphiCode(
$@"{{$INCLUDE {EscapeIncludeFileName(includeFile)}}}
"
             )), true, false).AppendDelphiCode(
$@"{{$IFDEF FPC}}
  {{$MODE DELPHI}}
{{$ENDIF}}

"
            ).AppendUsesClause(program.UsesClause);
            if (program.BlockDeclarations.Count > 0)
            {
                AppendDelphiCode(
$@"var
"
                );
                foreach (string line in program.BlockDeclarations) AppendDelphiCode(line, 1).AppendLine();
            }
            AppendDelphiCode(
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
