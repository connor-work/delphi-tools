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

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Work.Connor.Delphi.CodeWriter.Tests;

/// <summary>
/// Tests for <see cref="UnitIdentifier"/>.
/// </summary>
public class UnitIdentifierTest
{
    /// <summary>
    /// Known source code representation of a <see cref="Delphi.UnitIdentifier"/>.
    /// </summary>
    public sealed class KnownSourceCode
    {
        /// <summary>
        /// The <see cref="Delphi.UnitIdentifier"/>.
        /// </summary>
        public required UnitIdentifier UnitIdentifier { get; init; }

        /// <summary>
        /// Delphi identifier string that represents <see cref="UnitIdentifier"/>.
        /// </summary>
        public required string SourceCode { get; init; }
    }

    /// <summary>
    /// Known source code representation of <see cref="Delphi.UnitIdentifier"/>s.
    /// </summary>
    public static IEnumerable<KnownSourceCode> KnownSourceCodeValues => [
        new()
        {
            SourceCode = "Namespace1.Namespace2.uTest",
            UnitIdentifier = new UnitIdentifier
            {
                Namespace = {
                    "Namespace1",
                    "Namespace2",
                },
                Unit = "uTest",
            },
        },
        new()
        {
            SourceCode = "uTest",
            UnitIdentifier = new UnitIdentifier
            {
                Unit = "uTest",
            },
        }
    ];

    /// <summary>
    /// <see cref="KnownSourceCodeValues"/> as xUnit test vectors.
    /// </summary>
    public static IEnumerable<object[]> KnownSourceCodeValuesXUnit => KnownSourceCodeValues.Select(value => new object[] { value });

    /// <summary>
    /// <see cref="UnitIdentifier.ToSourceCode()"> is correct.
    /// </summary>
    /// <param name="knownSourceCode">Known source code representation</param>
    [Theory]
    [MemberData(nameof(KnownSourceCodeValuesXUnit))]
    public void ToSourceCode(KnownSourceCode knownSourceCode) => Assert.Equal(knownSourceCode.SourceCode, knownSourceCode.UnitIdentifier.ToSourceCode());

    /// <summary>
    /// <see cref="UnitIdentifier(string)"> is correct.
    /// </summary>
    /// <param name="knownSourceCode">Known source code representation</param>
    [Theory]
    [MemberData(nameof(KnownSourceCodeValuesXUnit))]
    public void ConstructFromSourceCode(KnownSourceCode knownSourceCode) => Assert.Equal(knownSourceCode.UnitIdentifier, new UnitIdentifier(knownSourceCode.SourceCode));

    /// <summary>
    /// <see cref="UnitIdentifier.op_Implicit(string)"> is correct.
    /// </summary>
    /// <param name="knownSourceCode">Known source code representation</param>
    [Theory]
    [MemberData(nameof(KnownSourceCodeValuesXUnit))]
    public void ImplicitCastFromString(KnownSourceCode knownSourceCode)
    {
        UnitIdentifier unitIdentifier = knownSourceCode.SourceCode;
        Assert.Equal(knownSourceCode.UnitIdentifier, unitIdentifier);
    }
}
