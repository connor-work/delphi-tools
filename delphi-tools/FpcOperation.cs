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
    /// Represents a planned invocation of the Free Pascal Compiler (FPC)
    /// </summary>
    public class FpcOperation : DelphiCompilerOperation
    {
        /// <summary>
        /// Optional location of the <c>fpc</c> executable.
        /// If this value is absent, the executable is located by the system (usually using working directory and <c>PATH</c>).
        /// </summary>
        public string? FpcExecutablePath { get; set; }

        /// <summary>
        /// Constructs a new planned FPC invocation.
        /// </summary>
        /// <param name="inputFile">FPC input file, see <see cref="InputFile"/></param>
        public FpcOperation(string inputFile) : base(inputFile) { }

        public override (bool success, int exitCode, string? errorText) Perform()
        {
            using Process fpc = new Process();
            // By default, fpc resides in PATH
            fpc.StartInfo.FileName = FpcExecutablePath ?? GetExecutableName("fpc");
            if (GenerateDebugInfo) fpc.StartInfo.ArgumentList.Add("-g");
            foreach (string unitPathFolder in UnitPath) fpc.StartInfo.ArgumentList.Add($"-Fu{unitPathFolder}");
            foreach (string includePathFolder in IncludePath) fpc.StartInfo.ArgumentList.Add($"-Fi{includePathFolder}");
            if (OutputPath != null) fpc.StartInfo.ArgumentList.Add($"-FE{OutputPath}");
            fpc.StartInfo.ArgumentList.Add(InputFile);
            fpc.StartInfo.CreateNoWindow = true;
            fpc.StartInfo.UseShellExecute = false;
            fpc.StartInfo.RedirectStandardOutput = true;
            StringBuilder error = new StringBuilder();
            fpc.Start();
            fpc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) { error.AppendLine(e.Data); };
            fpc.BeginOutputReadLine();
            fpc.WaitForExit();
            return (fpc.ExitCode == 0, fpc.ExitCode, error.Length == 0 ? null : error.ToString());
        }
    }
}
