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
using System.Reflection;
using Xunit;

namespace Work.Connor.Delphi.CodeWriter.Tests
{
    /// <summary>
    /// Tests <see cref="CodeWriter"/> with known inputs and outputs.
    /// </summary>
    public class KnownAnswerTest
    {
        /// <summary>
        /// Formatter settings for encoding protobuf messages as JSON for test data. Can be used when creating new test vectors.
        /// </summary>
        public static readonly JsonFormatter.Settings protobufJsonFormatSettings = JsonFormatter.Settings.Default.WithFormatDefaultValues(false).WithFormatEnumsAsIntegers(false);

        /// <summary>
        /// Parser settings for decoding protobuf messages from JSON for test data
        /// </summary>
        public static readonly JsonParser.Settings protobufJsonParseSettings = JsonParser.Settings.Default.WithIgnoreUnknownFields(false);

        /// <summary>
        /// File name extension (without leading dot) for JSON-encoded protobuf messages in test data
        /// </summary>
        public static readonly string protobufJsonFileExtension = "pb.json";

        /// <summary>
        /// The Delphi Source Code Writer emits the expected Delphi source code for a Unit message.
        /// </summary>
        /// <param name="unit">Delphi Unit message</param>
        /// <param name="expectedCode">Expected source code string</param>
        [Theory]
        [MemberData(nameof(KnownUnitSourceCode))]
        public void ProducesExpectedUnitSourceCode(Unit unit, string expectedCode) => Assert.Equal(expectedCode, unit.ToSourceCode());

        /// <summary>
        /// Provides known pairs of Delphi Unit messages and corresponding source code.
        /// </summary>
        /// <returns>[0]: Delphi Unit message [1]: Expected source code string</returns>
        public static IEnumerable<object[]> KnownUnitSourceCode()
        {
            JsonParser jsonParser = new JsonParser(protobufJsonParseSettings);
            Assembly assembly = Assembly.GetExecutingAssembly();
            // Resources contain pairs of JSON-encoded Unit messages and expected unit source files
            foreach (string sourceCodeFileName in assembly.GetManifestResourceNames().Where(name => name.EndsWith(DelphiSourceCodeWriter.unitSourceFileExtension)))
            {
                string unitMessageFileName = sourceCodeFileName.Substring(0, sourceCodeFileName.Length - DelphiSourceCodeWriter.unitSourceFileExtension.Length) + protobufJsonFileExtension;
                using StreamReader unitMessageReader = new StreamReader(assembly.GetManifestResourceStream(unitMessageFileName) ?? throw new FileNotFoundException(unitMessageFileName));
                using StreamReader sourceCodeReader = new StreamReader(assembly.GetManifestResourceStream(sourceCodeFileName));
                yield return new object[] { jsonParser.Parse<Unit>(unitMessageReader), sourceCodeReader.ReadToEnd() };
            }
        }
    }
}
