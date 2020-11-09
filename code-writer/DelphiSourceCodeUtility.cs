using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;

namespace Work.Connor.Delphi
{
    /// <summary>
    /// Comparer that sorts sequences of comparables lexicographically.
    /// </summary>
    /// <typeparam name="T">The type of enumerable elements</typeparam>
    internal class EnumerableComparer<T> : Comparer<IEnumerable<T>?> where T : IComparable<T>
    {
        public override int Compare(IEnumerable<T>? x, IEnumerable<T>? y)
        {
            if (x is null) return y is null ? 0 : -1;
            if (y is null) return 1;
            foreach ((T leftElement, T rightElement) in x.Zip(y))
            {
                int elementCompare = leftElement.CompareTo(rightElement);
                if (elementCompare != 0) return elementCompare;
            }
            return x.Count().CompareTo(y.Count());
        }
    }

    public sealed partial class UnitReference : IComparable<UnitReference?>
    {
        /// <summary>
        /// Lexicographical comparer for string sequences
        /// </summary>
        private static EnumerableComparer<string> stringSequenceComparer = new EnumerableComparer<string>();

        // Implement lexicographical comparison of unit references

        public int CompareTo(UnitReference? other)
        {
            if (other == null) return 1;
            return stringSequenceComparer.Compare(Unit.Namespace.Append(Unit.Unit),
                                                  other.Unit.Namespace.Append(other.Unit.Unit));
        }

        public static bool operator ==(UnitReference? left, UnitReference? right) => (left is null) ? (right is null)
                                                                                                    : left.Equals(right);

        public static bool operator >(UnitReference? left, UnitReference? right) => (left is object) && left.CompareTo(right) > 0;

        public static bool operator <(UnitReference? left, UnitReference? right) => (left is null) ? (right is object)
                                                                                                   : left.CompareTo(right) > 0;

        public static bool operator >=(UnitReference? left, UnitReference? right) => (left == right) || (left < right);

        public static bool operator <=(UnitReference? left, UnitReference? right) => (left == right) || (left > right);

        public static bool operator !=(UnitReference? left, UnitReference? right) => !(left == right);
    }
    
    public sealed partial class ConditionalUnitReference : IComparable<ConditionalUnitReference?>
    {
        // Implement lexicographical comparison of conditionally compiled unit references

        public int CompareTo(ConditionalUnitReference? other) => other == null ? 1
                                                                               : ElementForComparable.CompareTo(other.ElementForComparable);

        /// <summary>
        /// Source code element that is used for lexicographical comparison.
        /// </summary>
        private UnitReference ElementForComparable => Element ?? AlternativeElement;

        public static bool operator ==(ConditionalUnitReference? left, ConditionalUnitReference? right) => (left is null) ? (right is null)
                                                                                                                          : left.Equals(right);

        public static bool operator >(ConditionalUnitReference? left, ConditionalUnitReference? right) => (left is object) && left.CompareTo(right) > 0;

        public static bool operator <(ConditionalUnitReference? left, ConditionalUnitReference? right) => (left is null) ? (right is object)
                                                                                                                         : left.CompareTo(right) > 0;

        public static bool operator >=(ConditionalUnitReference? left, ConditionalUnitReference? right) => (left == right) || (left < right);

        public static bool operator <=(ConditionalUnitReference? left, ConditionalUnitReference? right) => (left == right) || (left > right);

        public static bool operator !=(ConditionalUnitReference? left, ConditionalUnitReference? right) => !(left == right);
    }

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
