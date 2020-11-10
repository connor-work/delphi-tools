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

using System.Diagnostics;
using System.Text;

namespace Work.Connor.Delphi.Tools
{
    /// <summary>
    /// Represents a planned invocation of <a href="http://docwiki.embarcadero.com/RADStudio/Sydney/en/DCC64">DCC64</a>, the Embarcadero RAD Studio Delphi compiler for 64-bit Windows
    /// </summary>
    public class Dcc64Operation : DelphiCompilerOperation
    {
        /// <summary>
        /// Optional location of the <c>DCC64</c> executable.
        /// If this value is absent, the executable is located by the system (usually using working directory and <c>PATH</c>).
        /// </summary>
        public string? Dcc64ExecutablePath { get; set; }

        /// <summary>
        /// Constructs a new planned DCC64 invocation.
        /// </summary>
        /// <param name="inputFile">DCC64 input file, see <see cref="InputFile"/></param>
        public Dcc64Operation(string inputFile) : base(inputFile) { }

        public override (bool success, int exitCode, string? errorText) Perform()
        {
            using Process dcc64 = new Process();
            // By default, DCC64 resides in PATH
            dcc64.StartInfo.FileName = Dcc64ExecutablePath ?? GetExecutableName("dcc64");
            if (GenerateDebugInfo) dcc64.StartInfo.ArgumentList.Add("-V");
            foreach (string unitPathFolder in UnitPath) dcc64.StartInfo.ArgumentList.Add($"-U{unitPathFolder}");
            foreach (string includePathFolder in IncludePath) dcc64.StartInfo.ArgumentList.Add($"-I{includePathFolder}");
            if (OutputPath != null) dcc64.StartInfo.ArgumentList.Add($"-E{OutputPath}");
            dcc64.StartInfo.ArgumentList.Add(InputFile);
            dcc64.StartInfo.CreateNoWindow = true;
            dcc64.StartInfo.UseShellExecute = false;
            dcc64.StartInfo.RedirectStandardOutput = true;
            StringBuilder error = new StringBuilder();
            // TODO error or output?
            dcc64.Start();
            dcc64.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) { error.AppendLine(e.Data); };
            dcc64.BeginOutputReadLine();
            dcc64.WaitForExit();
            return (dcc64.ExitCode == 0, dcc64.ExitCode, error.Length == 0 ? null : error.ToString());
        }
    }
}
