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
        /// File name extension (without leading dot) for JSON-encoded protobuf messages in test data
        /// </summary>
        public static readonly string protobufJsonFileExtension = "pb.json";

        /// <summary>
        /// Resource set of all Delphi unit-related test resource files for this kind of test
        /// </summary>
        private static readonly IResourceSet delphiUnitTestResources = IResourceSet.Root.Nest("[known delphi unit]");

        /// <summary>
        /// Resource set of all Delphi program-related test resource files for this kind of test
        /// </summary>
        private static readonly IResourceSet delphiProgramTestResources = IResourceSet.Root.Nest("[known delphi program]");

        /// <summary>
        /// Resource set of all test resource files that define expected Delphi unit source code
        /// </summary>
        private static readonly IResourceSet allExpectedUnitSourceResources = delphiUnitTestResources.Nest("[expected source]");

        /// <summary>
        /// Resource set of all test resource files that define expected Delphi program source code
        /// </summary>
        private static readonly IResourceSet allExpectedProgramSourceResources = delphiProgramTestResources.Nest("[expected source]");

        /// <summary>
        /// Resource set of all test resource files containing protobuf messages that specify Delphi unit source code
        /// </summary>
        private static readonly IResourceSet allUnitMessageResources = delphiUnitTestResources.Nest("[message]");

        /// <summary>
        /// Resource set of all test resource files containing protobuf messages that specify Delphi program source code
        /// </summary>
        private static readonly IResourceSet allProgramMessageResources = delphiProgramTestResources.Nest("[message]");

        /// <summary>
        /// Names of all known Delphi unit-related test vectors
        /// </summary>
        private static IEnumerable<string> DelphiUnitTestVectorNames => allExpectedUnitSourceResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{DelphiSourceCodeWriter.unitSourceFileExtension}")));

        /// <summary>
        /// Names of all known Delphi program-related test vectors
        /// </summary>
        private static IEnumerable<string> DelphiProgramTestVectorNames => allExpectedProgramSourceResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{DelphiSourceCodeWriter.programSourceFileExtension}")));


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
        public class DelphiUnitTestVector : IXunitSerializable
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
            public DelphiUnitTestVector() { }
#pragma warning restore CS8618

            /// <summary>
            /// Constructs a new test vector.
            /// </summary>
            /// <param name="name">Name of the test vector</param>
            public DelphiUnitTestVector(string name) => this.name = name;

            /// <summary>
            /// Protobuf message specifying a Delphi unit
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
        public class DelphiProgramTestVector : IXunitSerializable
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
            public DelphiProgramTestVector() { }
#pragma warning restore CS8618

            /// <summary>
            /// Constructs a new test vector.
            /// </summary>
            /// <param name="name">Name of the test vector</param>
            public DelphiProgramTestVector(string name) => this.name = name;

            /// <summary>
            /// Protobuf message specifying a Delphi program
            /// </summary>
            public Program Program
            {
                get
                {
                    string resourceName = $"{name}.{protobufJsonFileExtension}";
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
        /// All known Delphi unit-related test vectors
        /// </summary>
        public static IEnumerable<object[]> DelphiUnitTestVectors => DelphiUnitTestVectorNames.Select(name => new object[] { new DelphiUnitTestVector(name) });

        /// <summary>
        /// All known Delphi program-related test vectors
        /// </summary>
        public static IEnumerable<object[]> DelphiProgramTestVectors => DelphiProgramTestVectorNames.Select(name => new object[] { new DelphiProgramTestVector(name) });

        /// <summary>
        /// The Delphi Source Code Writer emits the expected Delphi source code for a Unit message.
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(DelphiUnitTestVectors))]
        public void ProducesExpectedUnitSourceCode(DelphiUnitTestVector vector) => Assert.Equal(vector.ExpectedSourceCode, vector.Unit.ToSourceCode());

        /// <summary>
        /// The Delphi Source Code Writer emits the expected Delphi source code for a Program message.
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(DelphiProgramTestVectors))]
        public void ProducesExpectedProgramSourceCode(DelphiProgramTestVector vector) => Assert.Equal(vector.ExpectedSourceCode, vector.Program.ToSourceCode());
    }
}
