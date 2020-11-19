/// Copyright 2020 Connor Roehricht (connor.work)
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

using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;

namespace Work.Connor.Delphi.CodeWriter
{
    /// <summary>
    /// Extensions to Delphi source code types for convenient source code manipulation.
    /// </summary>
    public static partial class SourceCodeExtensions
    {
        /// <summary>
        /// Sorts a Delphi uses clause in-place.
        /// </summary>
        /// <param name="usesClause">The uses clause to sort</param>
        public static void SortUsesClause(this RepeatedField<ConditionalUnitReference> usesClause)
        {
            RepeatedField<ConditionalUnitReference> oldClause = usesClause.Clone();
            usesClause.Clear();
            usesClause.AddRange(oldClause.OrderBy(reference => reference));
        }

        /// <summary>
        /// Adds an unconditionally compiled source code element to a sequence of conditionally compiled elements.
        /// </summary>
        /// <param name="sequence">The sequence append to</param>
        /// <param name="element">The element to append</param>
        public static void Add(this RepeatedField<ConditionalUnitReference> sequence, UnitReference element) => sequence.Add(new ConditionalUnitReference() { Element = element });

        /// <summary>
        /// Adds a sequence of unconditionally compiled source code elements to a sequence of conditionally compiled elements.
        /// </summary>
        /// <param name="sequence">The sequence append to</param>
        /// <param name="elements">The elements to append</param>
        public static void Add(this RepeatedField<ConditionalUnitReference> sequence, IEnumerable<UnitReference> elements) => sequence.Add(elements.Select(element => new ConditionalUnitReference() { Element = element }));

        /// <summary>
        /// Adds an unconditionally compiled source code element to a sequence of conditionally compiled elements.
        /// </summary>
        /// <param name="sequence">The sequence append to</param>
        /// <param name="element">The element to append</param>
        public static void Add(this RepeatedField<ConditionalAttributeAnnotation> sequence, AttributeAnnotation element) => sequence.Add(new ConditionalAttributeAnnotation() { Element = element });

        /// <summary>
        /// Adds a sequence of unconditionally compiled source code elements to a sequence of conditionally compiled elements.
        /// </summary>
        /// <param name="sequence">The sequence append to</param>
        /// <param name="elements">The elements to append</param>
        public static void Add(this RepeatedField<ConditionalAttributeAnnotation> sequence, IEnumerable<AttributeAnnotation> elements) => sequence.Add(elements.Select(element => new ConditionalAttributeAnnotation() { Element = element }));
    }
}
