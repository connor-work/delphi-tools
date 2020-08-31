using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Work.Connor.Delphi.CodeWriter.Tests
{
    /// <summary>
    /// Tests <see cref="CodeWriter"/> with known inputs and outputs.
    /// </summary>
    public class KnownAnswerTest
    {
        /// <summary>
        /// Formatter settings for encoding protobuf messages as JSON for test data. Can be used when creating new test files.
        /// </summary>
        public static readonly JsonFormatter.Settings protobufJsonFormatSettings = JsonFormatter.Settings.Default.WithFormatDefaultValues(false).WithFormatEnumsAsIntegers(false);

        /// <summary>
        /// Parser settings for decoding protobuf messages from JSON for test data
        /// </summary>
        public static readonly JsonParser.Settings protobufJsonParseSettings = JsonParser.Settings.Default.WithIgnoreUnknownFields(false);

        /// <summary>
        /// File name extension for JSON-encoded protobuf messages in test data
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
            string[] names = assembly.GetManifestResourceNames();
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
