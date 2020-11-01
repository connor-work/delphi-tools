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
using Work.Connor.Delphi.Tools;
using Xunit;
using Xunit.Abstractions;

namespace Work.Connor.Delphi.CodeWriter.Tests
{
    /// <summary>
    /// Tests if Delphi code produced by <see cref="DelphiSourceCodeWriter"/> can be compiled.
    /// </summary>
    public class DelphiCompilabilityTest
    {
        /// <summary>
        /// Resource set of all test resource files that encode protobuf messages representing Delphi unit source code
        /// </summary>
        private static readonly IResourceSet allUnitMessageResources = IResourceSet.Root.Nest("[known delphi unit message]");

        /// <summary>
        /// Resource set of all test resource files that encode protobuf messages representing Delphi program source code
        /// </summary>
        private static readonly IResourceSet allProgramMessageResources = IResourceSet.Root.Nest("[known delphi program message]");

        /// <summary>
        /// Resource set of all Delphi units that contain support source code for testing
        /// </summary>
        private static readonly IResourceSet testSupportCodeUnitResources = IResourceSet.Root.Nest("[Delphi test support code unit]");

        /// <summary>
        /// Names of all known test vectors for Delphi units
        /// </summary>
        private static IEnumerable<string> UnitTestVectorNames => allUnitMessageResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{protobufJsonFileExtension}")));

        /// <summary>
        /// Names of all known test vectors for Delphi programs
        /// </summary>
        private static IEnumerable<string> ProgramTestVectorNames => allProgramMessageResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{protobufJsonFileExtension}")));

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
        /// Utility function to create a temporary scratch folder for testing.
        /// </summary>
        /// <returns>Path of the new folder</returns>
        private static string CreateScratchFolder()
        {
            string path = Path.GetTempFileName();
            File.Delete(path);
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Represents a test vector for a Delphi unit for this kind of test
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
            /// Name of the test vector
            /// </summary>
            public string Name => name;

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

            public void Deserialize(IXunitSerializationInfo info) => name = info.GetValue<string>(nameof(name));

            public void Serialize(IXunitSerializationInfo info) => info.AddValue(nameof(name), name);

            public override string? ToString() => name;
        }

        /// <summary>
        /// Represents a test vector for a Delphi program for this kind of test
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
            /// Name of the test vector
            /// </summary>
            public string Name => name;

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
                    using StreamReader reader = new StreamReader(allProgramMessageResources.GetResourceStream(resourceName) ?? throw new FileNotFoundException(resourceName));
                    return jsonParser.Parse<Program>(reader);
                }
            }

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
        /// <see cref="DelphiSourceCodeWriter"/> produces Delphi unit source code that can be compiled using FPC
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(UnitTestVectors))]
        public void ProducesUnitSourceThatCanBeCompiled(UnitTestVector vector)
        {
            // TODO this test should actually be skipped, waiting for xUnit support https://github.com/xunit/xunit/issues/2073#issuecomment-673632823
            // Vector only skipped until FPC attribute feature (in 3.3.1 preview) is available https://wiki.freepascal.org/Custom_Attributes
            if (vector.Name == "uAttributes") return;

            // Write the unit source code
            Unit unit = vector.Unit;
            string sourceCode = unit.ToSourceCode();

            // Create a test runner program as input for FPC
            string programFile = Path.Join(CreateScratchFolder(), "DelphiCompilationTestProgram.pas");
            Program program = new Program()
            {
                Heading = "DelphiCompilationTestProgram",
                UsesClause = { new UnitReference() { Unit = unit.Heading.Clone() } }
            };
            File.WriteAllText(programFile, program.ToSourceCode());

            // Run FPC
            FpcOperation fpc = new FpcOperation(programFile) { OutputPath = CreateScratchFolder() };
            // Adds units from a resource set to FPC
            void addUnits(IEnumerable<(string name, string content)> resources, string rootFolder)
            {
                foreach ((string name, string content) in resources)
                {
                    string path = Path.Join(rootFolder, name);
                    string folder = Directory.GetParent(path).FullName;
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(path, content);
                    if (!fpc.UnitPath.Contains(folder)) fpc.UnitPath.Add(folder);
                }
            }
            // Add written source code
            addUnits(new (string, string)[] { (Path.Join(unit.ToSourceFilePath().ToArray()), sourceCode) }, CreateScratchFolder());
            // Add support files (may contain required source code)
            addUnits(testSupportCodeUnitResources.ReadAllResources(), CreateScratchFolder());
            (bool fpcSuccess, _, string? fpcError) = fpc.Perform();
            Assert.True(fpcSuccess, fpcError!);
        }

        /// <summary>
        /// <see cref="DelphiSourceCodeWriter"/> produces Delphi program source code that can be compiled using FPC
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(ProgramTestVectors))]
        public void ProducesProgramSourceThatCanBeCompiled(ProgramTestVector vector)
        {
            // Write the program source code
            Program program = vector.Program;
            string programFile = Path.Join(CreateScratchFolder(), Path.Join(program.ToSourceFilePath().ToArray()));
            File.WriteAllText(programFile, program.ToSourceCode());

            // Run FPC
            FpcOperation fpc = new FpcOperation(programFile) { OutputPath = CreateScratchFolder() };
            // Adds units from a resource set to FPC
            void addUnits(IEnumerable<(string name, string content)> resources, string rootFolder)
            {
                foreach ((string name, string content) in resources)
                {
                    string path = Path.Join(rootFolder, name);
                    string folder = Directory.GetParent(path).FullName;
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(path, content);
                    if (!fpc.UnitPath.Contains(folder)) fpc.UnitPath.Add(folder);
                }
            }
            // Add support files (may contain required source code)
            addUnits(testSupportCodeUnitResources.ReadAllResources(), CreateScratchFolder());
            (bool fpcSuccess, _, string? fpcError) = fpc.Perform();
            Assert.True(fpcSuccess, fpcError!);
        }
    }
}
