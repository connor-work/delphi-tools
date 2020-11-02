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

using Google.Protobuf.Collections;
using Xunit;

namespace Work.Connor.Delphi.CodeWriter.Tests
{
    /// <summary>
    /// Tests Delphi source code utility code.
    /// </summary>
    public class DelphiSourceCodeUtilityTest
    {
        /// <summary>
        /// <see cref="Delphi.SourceCodeExtensions.SortUsesClause(RepeatedField{UnitReference})"/> sorts a uses clause correctly.
        /// </summary>
        [Fact]
        public void SortsUsesClauseCorrectly()
        {
            RepeatedField<UnitReference> unsortedClause = new RepeatedField<UnitReference>
            {
                new UnitReference() { Unit = new UnitIdentifier() { Unit = "SysUtils", Namespace = { "System" } } },
                new UnitReference() { Unit = new UnitIdentifier() { Unit = "Collections", Namespace = { "System", "Generics" } } },
                new UnitReference() { Unit = new UnitIdentifier() { Unit = "System" } }
            };
            RepeatedField<UnitReference> sortedClause = new RepeatedField<UnitReference>
            {
                unsortedClause[2], unsortedClause[1], unsortedClause[0]
            };
            unsortedClause.SortUsesClause();
            Assert.Equal(sortedClause, unsortedClause);
        }
    }
}
