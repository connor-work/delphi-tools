/// Copyright 2026 Connor Erdmann (connor.work)
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Work.Connor.Delphi;

public sealed partial class UnitIdentifier
{
    /// <summary>
    /// Regular expression for a Delphi identifier string that represents a <see cref="UnitIdentifier"/>.
    /// </summary>
    /// <returns>The regular expression</returns>
    [GeneratedRegex("^(?:(?<namespaceElement>[A-Za-z][A-Za-z0-9]*)\\.)*(?<unit>[A-Za-z][A-Za-z0-9]*)$")]
    private static partial Regex UnitIdentifierRegex();

    /// <summary>
    /// Constructs a <see cref="UnitIdentifier"/> from a Delphi identifier string that represents it.
    /// </summary>
    /// <param name="sourceCode">The Delphi identifier string</param>
    public UnitIdentifier(string sourceCode) : this()
    {
        Match match = UnitIdentifierRegex().Match(sourceCode);
        if (!match.Success) throw new ArgumentException("Invalid source code", nameof(sourceCode));
        Namespace.AddRange(match.Groups["namespaceElement"].Captures.Select(capture => capture.Value));
        Unit = match.Groups["unit"].Value;
    }

    /// <summary>
    /// Constructs a <see cref="UnitIdentifier"/> from a Delphi identifier string that represents it
    /// </summary>
    /// <param name="sourceCode">The Delphi identifier string</param>
    [return: NotNullIfNotNull(nameof(sourceCode))]
    public static implicit operator UnitIdentifier?(string? sourceCode) => sourceCode is null ? null : new(sourceCode);

    /// <summary>
    /// Constructs a Delphi identifier string for this Delphi unit identifier.
    /// </summary>
    /// <returns>The Delphi identifier string</returns>
    public string ToSourceCode() => string.Join(".", Namespace.Append(Unit));
}
