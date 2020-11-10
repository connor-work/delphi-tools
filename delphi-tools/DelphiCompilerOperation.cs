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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Work.Connor.Delphi.Tools
{
    /// <summary>
    /// Represents a planned invocation of a Delphi compiler
    /// </summary>
    public abstract class DelphiCompilerOperation
    {
        /// <summary>
        /// <i>Unit path</i> containing units that can be used by compiled programs (directly or transitively)
        /// </summary>
        public List<string> UnitPath { get; } = new List<string>();

        /// <summary>
        /// <i>Include path</i> containing include files that can be used by compiled programs or units
        /// </summary>
        public List<string> IncludePath { get; } = new List<string>();

        /// <summary>
        /// <i>Output path</i> for compiler outputs
        /// </summary>
        public string? OutputPath { get; set; }

        /// <summary>
        /// <i>Input file</i> for the compiler
        /// </summary>
        public string InputFile { get; }

        /// <summary>
        /// <see langword="true"/> if debug information shall be generated.
        /// </summary>
        public bool GenerateDebugInfo { get; set; } = false;

        /// <summary>
        /// Constructs an executable file name for the current platform.
        /// </summary>
        /// <param name="name">The base name, without extension</param>
        /// <returns>The executable file name</returns>
        protected static string GetExecutableName(string name)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"{name}.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return name;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return name;
            throw new NotImplementedException("Unsupported OS");
        }

        /// <summary>
        /// Constructs a new planned compiler invocation.
        /// </summary>
        /// <param name="compiler">The Delphi compiler</param>
        /// <param name="inputFile">Compiler input file, see <see cref="InputFile"/></param>
        /// <returns></returns>
        public static DelphiCompilerOperation Plan(DelphiCompiler compiler, string inputFile) => compiler switch
        {
            DelphiCompiler.FPC => new FpcOperation(inputFile),
            DelphiCompiler.DCC64 => new Dcc64Operation(inputFile),
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Constructs a new planned compiler invocation.
        /// </summary>
        /// <param name="inputFile">Compiler input file, see <see cref="InputFile"/></param>
        protected DelphiCompilerOperation(string inputFile) => InputFile = inputFile;

        /// <summary>
        /// Performs the planned compiler invocation.
        /// </summary>
        /// <returns><see langword="true"/> if the operation succeeded, the exit code of the compiler and an optional error message</returns>
        public abstract (bool success, int exitCode, string? errorText) Perform();
    }
}
