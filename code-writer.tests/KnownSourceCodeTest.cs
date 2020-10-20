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

using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Work.Connor.Delphi.CodeWriter.Tests
{
    /// <summary>
    /// Tests <see cref="CodeWriter"/> with known protobuf input messages and expected Delphi source code.
    /// </summary>
    public class KnownSourceCodeTest
    {
        /// <summary>
        /// Resource set of all test resource files that define expected Delphi unit source code
        /// </summary>
        private static readonly IResourceSet allExpectedUnitSourceResources = IResourceSet.Root.Nest("[known delphi unit source]");

        /// <summary>
        /// Resource set of all test resource files that define expected Delphi program source code
        /// </summary>
        private static readonly IResourceSet allExpectedProgramSourceResources = IResourceSet.Root.Nest("[known delphi program source]");

        /// <summary>
        /// Resource set of all test resource files that encode protobuf messages representing Delphi unit source code
        /// </summary>
        private static readonly IResourceSet allUnitMessageResources = IResourceSet.Root.Nest("[known delphi unit message]");

        /// <summary>
        /// Resource set of all test resource files that encode protobuf messages representing Delphi program source code
        /// </summary>
        private static readonly IResourceSet allProgramMessageResources = IResourceSet.Root.Nest("[known delphi program message]");

        /// <summary>
        /// Names of all known test vectors for Delphi units
        /// </summary>
        private static IEnumerable<string> UnitTestVectorNames => allExpectedUnitSourceResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{DelphiSourceCodeWriter.unitSourceFileExtension}")));

        /// <summary>
        /// Names of all known test vectors for Delphi programs
        /// </summary>
        private static IEnumerable<string> ProgramTestVectorNames => allExpectedProgramSourceResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{DelphiSourceCodeWriter.programSourceFileExtension}")));

        /// <summary>
        /// File name extension (without leading dot) for JSON-encoded protobuf messages in test data
        /// </summary>
        public static readonly string protobufJsonFileExtension = "pb.json";

        /// <summary>
        /// Formatter settings for encoding protobuf messages as JSON for test data. Can be used when creating new test vectors.
        /// </summary>
        public static readonly JsonFormatter.Settings protobufJsonFormatSettings = JsonFormatter.Settings.Default.WithFormatDefaultValues(false).WithFormatEnumsAsIntegers(false);

        /// <summary>
        /// Parser settings for decoding protobuf messages from JSON for test data
        /// </summary>
        public static readonly JsonParser.Settings protobufJsonParseSettings = JsonParser.Settings.Default.WithIgnoreUnknownFields(false);

        /// <summary>
        /// Represents a Delphi unit-related test vector for this kind of test
        /// </summary>
        public class UnitTestVector : IXunitSerializable
        {
            /// <summary>
            /// Parser for JSON-encoded protobuf test data
            /// </summary>
            private static readonly JsonParser jsonParser = new JsonParser(protobufJsonParseSettings);

            /// <summary>
            /// Name of the test vector
            /// </summary>
            private string name;

            /// <summary>
            /// Constructs a new test vector for deserialization by xUnit.
            /// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. -> Initialized during deserialization by xUnit
            public UnitTestVector() { }
#pragma warning restore CS8618

            /// <summary>
            /// Constructs a new test vector.
            /// </summary>
            /// <param name="name">Name of the test vector</param>
            public UnitTestVector(string name) => this.name = name;

            /// <summary>
            /// Protobuf message representing the Delphi unit source code
            /// </summary>
            public Unit Unit
            {
                get
                {
                    string resourceName = $"{name}.{protobufJsonFileExtension}";
                    using StreamReader reader = new StreamReader(allUnitMessageResources.GetResourceStream(resourceName) ?? throw new FileNotFoundException(resourceName));
                    return jsonParser.Parse<Unit>(reader);
                }
            }

            /// <summary>
            /// Expected Delphi source code for <see cref="Unit"/>
            /// </summary>
            public string ExpectedSourceCode => allExpectedUnitSourceResources.ReadResource($"{name}.{DelphiSourceCodeWriter.unitSourceFileExtension}")!;

            public void Deserialize(IXunitSerializationInfo info) => name = info.GetValue<string>(nameof(name));

            public void Serialize(IXunitSerializationInfo info) => info.AddValue(nameof(name), name);

            public override string? ToString() => name;
        }

        /// <summary>
        /// Represents a Delphi program-related test vector for this kind of test
        /// </summary>
        public class ProgramTestVector : IXunitSerializable
        {
            /// <summary>
            /// Parser for JSON-encoded protobuf test data
            /// </summary>
            private static readonly JsonParser jsonParser = new JsonParser(protobufJsonParseSettings);

            /// <summary>
            /// Name of the test vector
            /// </summary>
            private string name;

            /// <summary>
            /// Constructs a new test vector for deserialization by xUnit.
            /// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. -> Initialized during deserialization by xUnit
            public ProgramTestVector() { }
#pragma warning restore CS8618

            /// <summary>
            /// Constructs a new test vector.
            /// </summary>
            /// <param name="name">Name of the test vector</param>
            public ProgramTestVector(string name) => this.name = name;

            /// <summary>
            /// Protobuf message representing the Delphi program source code
            /// </summary>
            public Program Program
            {
                get
                {
                    string resourceName = $"{name}.{protobufJsonFileExtension}";
                    throw new Exception(string.Join(", ", allProgramMessageResources.GetIDs()));
                    using StreamReader reader = new StreamReader(allProgramMessageResources.GetResourceStream(resourceName) ?? throw new FileNotFoundException(resourceName));
                    return jsonParser.Parse<Program>(reader);
                }
            }

            /// <summary>
            /// Expected Delphi source code for <see cref="Program"/>
            /// </summary>
            public string ExpectedSourceCode => allExpectedProgramSourceResources.ReadResource($"{name}.{DelphiSourceCodeWriter.programSourceFileExtension}")!;

            public void Deserialize(IXunitSerializationInfo info) => name = info.GetValue<string>(nameof(name));

            public void Serialize(IXunitSerializationInfo info) => info.AddValue(nameof(name), name);

            public override string? ToString() => name;
        }

        /// <summary>
        /// All known test vectors for Delphi units
        /// </summary>
        public static IEnumerable<object[]> UnitTestVectors => UnitTestVectorNames.Select(name => new object[] { new UnitTestVector(name) });

        /// <summary>
        /// All known test vectors for Delphi programs
        /// </summary>
        public static IEnumerable<object[]> ProgramTestVectors => ProgramTestVectorNames.Select(name => new object[] { new ProgramTestVector(name) });

        /// <summary>
        /// <see cref="DelphiSourceCodeWriter"/> produces the expected Delphi source code for a protobuf message representing a Delphi unit
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(UnitTestVectors))]
        public void ProducesExpectedUnitSourceCode(UnitTestVector vector) => Assert.Equal(vector.ExpectedSourceCode, vector.Unit.ToSourceCode());

        /// <summary>
        /// <see cref="DelphiSourceCodeWriter"/> produces the expected Delphi source code for a protobuf message representing a Delphi program
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(ProgramTestVectors))]
        public void ProducesExpectedProgramSourceCode(ProgramTestVector vector) => Assert.Equal(vector.ExpectedSourceCode, vector.Program.ToSourceCode());
    }
}
