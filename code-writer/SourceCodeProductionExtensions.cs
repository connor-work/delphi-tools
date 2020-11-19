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
using System.Linq;

namespace Work.Connor.Delphi.CodeWriter
{
    /// <summary>
    /// Extensions to Delphi source code types for source code production.
    /// </summary>
    public static partial class SourceCodeProductionExtensions
    {
        /// <summary>
        /// Constructs the recommended source file path for a unit.
        /// </summary>
        /// <param name="unit">The unit</param>
        /// <returns>Sequence of path components for the recommended source file</returns>
        public static IEnumerable<string> ToSourceFilePath(this Unit unit) => unit.Heading.Namespace.Append($"{unit.Heading.ToSourceCode()}.pas");

        /// <summary>
        /// Constructs the recommended source file path for a program.
        /// </summary>
        /// <param name="program">The program</param>
        /// <returns>Sequence of path components for the recommended source file</returns>
        public static IEnumerable<string> ToSourceFilePath(this Program program) => new string[] { ($"{program.Heading}.pas") };

#pragma warning disable S4136 // Method overloads should be grouped together -> "ToSourceCode" method order reflects order in protobuf schema here

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
        /// Constructs a Delphi source code string for a Delphi RTTI attribute annotation.
        /// </summary>
        /// <param name="annotation">The attribute annotation</param>
        /// <returns>The Delphi source code string</returns>
        internal static string ToSourceCode(this AttributeAnnotation annotation) => $"[{annotation.Attribute}]";

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
}
