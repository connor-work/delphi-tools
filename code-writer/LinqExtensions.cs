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
using System.Linq;

namespace Work.Connor.Delphi.CodeWriter
{
    /// <summary>
    /// Extensions similar to <see cref="System.Linq"/>, used for functional operations
    /// </summary>
    internal static partial class LinqExtensions
    {
        /// <summary>
        /// Performs a partial application of a function for each element of a sequence.
        /// </summary>
        /// <typeparam name="T">Type of the sequence elements (function parameters)</typeparam>
        /// <param name="parameters">Sequence of function parameters</param>
        /// <param name="action">Function whose first parameter accepts a sequence element</param>
        /// <returns>Sequence of partially applied functions</returns>
        public static IEnumerable<Action> PartiallyApply<T>(this IEnumerable<T> parameters, Action<T> action) => parameters.Select(parameter => (Action)(() => action.Invoke(parameter)));
    }
}
