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

namespace Work.Connor.Protobuf.Delphi.CodeWriter.Tests
{
    /// <summary>
    /// Collection of configurable options for the Delphi Source Code Writer tests.
    /// </summary>
    public static class TestOptions
    {
        /// <summary>
        /// <see langword="true"/> if tests using <see cref="DelphiCompiler.DCC64"/> shall be skipped.
        /// </summary>
        public static bool DisableDCC64 => Environment.GetEnvironmentVariable("Work_Connor_Delphi_CodeWriter_Tests_SkipDcc64") == "1";
    }
}
