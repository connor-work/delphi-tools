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
using Work.Connor.Delphi.Tools;
using Work.Connor.Protobuf.Delphi.CodeWriter.Tests;
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
        /// Resource set of all Delphi include files that contain support source code for testing
        /// </summary>
        private static readonly IResourceSet testSupportCodeIncludeFileResources = IResourceSet.Root.Nest("[Delphi test support code include file]");

        /// <summary>
        /// Names of all known test message for Delphi units
        /// </summary>
        private static IEnumerable<string> UnitTestMessageNames => allUnitMessageResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{protobufJsonFileExtension}")));

        /// <summary>
        /// Names of all known test messages for Delphi programs
        /// </summary>
        private static IEnumerable<string> ProgramTestMessageNames => allProgramMessageResources.GetIDs().WhereSuffixed(new Regex(Regex.Escape($".{protobufJsonFileExtension}")));

        /// <summary>
        /// Delphi compilers used for testing
        /// </summary>
        private static IEnumerable<DelphiCompiler> TestCompilers => (DelphiCompiler[])  Enum.GetValues(typeof(DelphiCompiler));

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
            /// Name of the test message
            /// </summary>
            private string messageName;

            /// <summary>
            /// Compiler used for testing
            /// </summary>
            private DelphiCompiler compiler;

            /// <summary>
            /// Name of the test vector
            /// </summary>
            public string Name => $"{messageName}-{compiler}";

            /// <summary>
            /// Compiler used for testing
            /// </summary>
            public DelphiCompiler Compiler => compiler;

            /// <summary>
            /// Constructs a new test vector for deserialization by xUnit.
            /// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. -> Initialized during deserialization by xUnit
            public UnitTestVector() { }
#pragma warning restore CS8618

            /// <summary>
            /// Constructs a new test vector.
            /// </summary>
            /// <param name="messageName">Name of the test message</param>
            /// <param name="compiler">Compiler used for testing</param>
            public UnitTestVector(string messageName, DelphiCompiler compiler)
            {
                this.messageName = messageName;
                this.compiler = compiler;
            }

            /// <summary>
            /// Protobuf message representing the Delphi unit source code
            /// </summary>
            public Unit Unit
            {
                get
                {
                    string resourceName = $"{messageName}.{protobufJsonFileExtension}";
                    using StreamReader reader = new StreamReader(allUnitMessageResources.GetResourceStream(resourceName) ?? throw new FileNotFoundException(resourceName));
                    return jsonParser.Parse<Unit>(reader);
                }
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                messageName = info.GetValue<string>(nameof(messageName));
                compiler = info.GetValue<DelphiCompiler>(nameof(compiler));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(messageName), messageName);
                info.AddValue(nameof(compiler), compiler);
            }

            public override string? ToString() => Name;
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
            /// Name of the test message
            /// </summary>
            private string messageName;

            /// <summary>
            /// Compiler used for testing
            /// </summary>
            private DelphiCompiler compiler;

            /// <summary>
            /// Name of the test vector
            /// </summary>
            public string Name => $"{messageName}-{compiler}";

            /// <summary>
            /// Compiler used for testing
            /// </summary>
            public DelphiCompiler Compiler => compiler;

            /// <summary>
            /// Constructs a new test vector for deserialization by xUnit.
            /// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. -> Initialized during deserialization by xUnit
            public ProgramTestVector() { }
#pragma warning restore CS8618

            /// <summary>
            /// Constructs a new test vector.
            /// </summary>
            /// <param name="messageName">Name of the test message</param>
            /// <param name="compiler">Compiler used for testing</param>
            public ProgramTestVector(string messageName, DelphiCompiler compiler)
            {
                this.messageName = messageName;
                this.compiler = compiler;
            }

            /// <summary>
            /// Protobuf message representing the Delphi program source code
            /// </summary>
            public Program Program
            {
                get
                {
                    string resourceName = $"{messageName}.{protobufJsonFileExtension}";
                    using StreamReader reader = new StreamReader(allProgramMessageResources.GetResourceStream(resourceName) ?? throw new FileNotFoundException(resourceName));
                    return jsonParser.Parse<Program>(reader);
                }
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                messageName = info.GetValue<string>(nameof(messageName));
                compiler = info.GetValue<DelphiCompiler>(nameof(compiler));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(messageName), messageName);
                info.AddValue(nameof(compiler), compiler);
            }

            public override string? ToString() => Name;
        }

        /// <summary>
        /// All known test vectors for Delphi units
        /// </summary>
        public static IEnumerable<object[]> UnitTestVectors => UnitTestMessageNames.SelectMany(messageName => TestCompilers, (messageName, compiler) => new object[] { new UnitTestVector(messageName, compiler) });

        /// <summary>
        /// All known test vectors for Delphi programs
        /// </summary>
        public static IEnumerable<object[]> ProgramTestVectors => ProgramTestMessageNames.SelectMany(messageName => TestCompilers, (messageName, compiler) => new object[] { new ProgramTestVector(messageName, compiler) });

        /// <summary>
        /// <see cref="DelphiSourceCodeWriter"/> produces Delphi unit source code that can be compiled using FPC
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(UnitTestVectors))]
        public void ProducesUnitSourceThatCanBeCompiled(UnitTestVector vector)
        {
            // TODO this test should actually be skipped, waiting for xUnit support https://github.com/xunit/xunit/issues/2073#issuecomment-673632823
            if (vector.Compiler == DelphiCompiler.DCC64
             && TestOptions.DisableDCC64) return;
            // Vectors only skipped until FPC attribute feature (in 3.3.1 preview) is available https://wiki.freepascal.org/Custom_Attributes
            if (vector.Name == "uAttributes-FPC"
             || vector.Name == "uConditionalCompilation-FPC") return;

            // Write the unit source code
            Unit unit = vector.Unit;
            string sourceCode = unit.ToSourceCode();

            // Create a test runner program as input for the Delphi compiler
            string programFile = Path.Join(CreateScratchFolder(), "DelphiCompilationTestProgram.pas");
            Program program = new Program()
            {
                Heading = "DelphiCompilationTestProgram",
                UsesClause = { new UnitReference() { Unit = unit.Heading.Clone() } }
            };
            File.WriteAllText(programFile, program.ToSourceCode());

            // Run the Delphi compiler
            DelphiCompilerOperation compilation = DelphiCompilerOperation.Plan(vector.Compiler, programFile);
            compilation.OutputPath = CreateScratchFolder();
            // Adds units from a resource set to the compilation
            void addUnits(IEnumerable<(string name, string content)> resources, string rootFolder)
            {
                foreach ((string name, string content) in resources)
                {
                    string path = Path.Join(rootFolder, name);
                    string folder = Directory.GetParent(path).FullName;
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(path, content);
                    if (!compilation.UnitPath.Contains(folder)) compilation.UnitPath.Add(folder);
                }
            }
            // Adds include files from a resource set to the compilation
            void addIncludeFiles(IEnumerable<(string name, string content)> resources, string rootFolder)
            {
                foreach ((string name, string content) in resources)
                {
                    string path = Path.Join(rootFolder, name);
                    string folder = Directory.GetParent(path).FullName;
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(path, content);
                    if (!compilation.IncludePath.Contains(folder)) compilation.IncludePath.Add(folder);
                }
            }
            // Add written source code
            addUnits(new (string, string)[] { (Path.Join(unit.ToSourceFilePath().ToArray()), sourceCode) }, CreateScratchFolder());
            // Add support files (may contain required source code)
            addUnits(testSupportCodeUnitResources.ReadAllResources(), CreateScratchFolder());
            addIncludeFiles(testSupportCodeIncludeFileResources.ReadAllResources(), CreateScratchFolder());
            (bool compilationSuccess, _, string? fpcError) = compilation.Perform();
            Assert.True(compilationSuccess, fpcError!);
        }

        /// <summary>
        /// <see cref="DelphiSourceCodeWriter"/> produces Delphi program source code that can be compiled using FPC
        /// </summary>
        /// <param name="vector">Test vector</param>
        [Theory]
        [MemberData(nameof(ProgramTestVectors))]
        public void ProducesProgramSourceThatCanBeCompiled(ProgramTestVector vector)
        {
            // TODO this test should actually be skipped, waiting for xUnit support https://github.com/xunit/xunit/issues/2073#issuecomment-673632823
            if (vector.Compiler == DelphiCompiler.DCC64
             && TestOptions.DisableDCC64) return;

            // Write the program source code
            Program program = vector.Program;
            string programFile = Path.Join(CreateScratchFolder(), Path.Join(program.ToSourceFilePath().ToArray()));
            File.WriteAllText(programFile, program.ToSourceCode());

            // Run the Delphi compiler
            DelphiCompilerOperation compilation = DelphiCompilerOperation.Plan(vector.Compiler, programFile);
            compilation.OutputPath = CreateScratchFolder();
            // Adds units from a resource set to the compilation
            void addUnits(IEnumerable<(string name, string content)> resources, string rootFolder)
            {
                foreach ((string name, string content) in resources)
                {
                    string path = Path.Join(rootFolder, name);
                    string folder = Directory.GetParent(path).FullName;
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(path, content);
                    if (!compilation.UnitPath.Contains(folder)) compilation.UnitPath.Add(folder);
                }
            }
            // Adds include files from a resource set to the compilation
            void addIncludeFiles(IEnumerable<(string name, string content)> resources, string rootFolder)
            {
                foreach ((string name, string content) in resources)
                {
                    string path = Path.Join(rootFolder, name);
                    string folder = Directory.GetParent(path).FullName;
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(path, content);
                    if (!compilation.IncludePath.Contains(folder)) compilation.IncludePath.Add(folder);
                }
            }
            // Add support files (may contain required source code)
            addUnits(testSupportCodeUnitResources.ReadAllResources(), CreateScratchFolder());
            addIncludeFiles(testSupportCodeIncludeFileResources.ReadAllResources(), CreateScratchFolder());
            (bool fpcSuccess, _, string? fpcError) = compilation.Perform();
            Assert.True(fpcSuccess, fpcError!);
        }
    }
}
