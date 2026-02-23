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

using System.Diagnostics.CodeAnalysis;

namespace Work.Connor.Delphi;

public sealed partial class UnitReference
{
    /// <summary>
    /// Constructs a <see cref="UnitReference"/> from a Delphi identifier string that represents its <see cref="Unit"/>.
    /// </summary>
    /// <param name="unitSourceCode">The Delphi identifier string of the <see cref="Unit"/></param>
    public UnitReference(string unitSourceCode) : this() => Unit = new UnitIdentifier(unitSourceCode);

    /// <summary>
    /// Constructs a <see cref="UnitIdentifier"/> from a Delphi identifier string that represents its <see cref="Unit"/>.
    /// </summary>
    /// <param name="unitSourceCode">The Delphi identifier string of the <see cref="Unit"/></param>
    [return: NotNullIfNotNull(nameof(unitSourceCode))]
    public static implicit operator UnitReference?(string? unitSourceCode) => unitSourceCode is null ? null : new(unitSourceCode);

    /// <summary>
    /// Constructs a Delphi source code that represents this <see cref="UnitReference"/>.
    /// </summary>
    /// <returns>The Delphi source code</returns>
    public string ToSourceCode() => Unit.ToSourceCode();
}
