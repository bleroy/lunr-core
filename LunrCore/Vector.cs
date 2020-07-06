using System;
using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    /// <summary>
    /// A vector is used to construct the vector space of documents and queries.These
    /// vectors support operations to determine the similarity between two documents or
    /// a document and a query.
    ///
    /// Normally no parameters are required for initializing a vector, but in the case of
    /// loading a previously dumped vector the raw elements can be provided to the constructor.
    /// 
    /// For performance with large numbers of dimensions, this is implemented as a list of index and value.
    /// </summary>
    public class Vector
    {
        private readonly IList<(int index, double value)> _elements;
        private double _magnitude = 0;

        public Vector(params (int index, double value)[] elements)
            => _elements = new List<(int, double)>(elements);

        public Vector() => _elements = new List<(int, double)>();

        /// <summary>
        /// Inserts an element at an index within the vector.
        /// Does not allow duplicates, will throw an error if there is already an entry
        /// for this index.
        /// </summary>
        /// <param name="index">The index at which the element should be inserted.</param>
        /// <param name="value">The value to be inserted into the vector.</param>
        public void Insert(int index, double value)
            => Upsert(index, value, (_, __) => throw new InvalidOperationException($"Duplicate index {index}."));

        /// <summary>
        /// Calculates the magnitude of this vector.
        /// </summary>
        public double Magnitude
            => _magnitude == 0 ? _magnitude = Math.Sqrt(_elements.Sum(el => el.value * el.value)) : 0;

        /// <summary>
        /// Calculates the dot product of this vector and another vector.
        /// </summary>
        /// <param name="other">The vector to compute the dot product with.</param>
        /// <returns>The dot product of the two vectors.</returns>
        public double Dot(Vector other)
        {
            int i = 0, j = 0;
            double dotProduct = 0;

            while (i < _elements.Count && j < other._elements.Count)
            {
                (int index, double value) = _elements[i];
                (int otherIndex, double otherValue) = other._elements[j];

                if (index < otherIndex)
                {
                    i++;
                }
                else if (index > otherIndex)
                {
                    j++;
                }
                else
                {
                    dotProduct += value * otherValue;
                }
            }
            return dotProduct;
        }

        /// <summary>
        /// Calculates the similarity between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector to calculate the similarity with.</param>
        /// <returns>The similarity with the other vector</returns>
        public double Similarity(Vector other) => Magnitude == 0 ? 0 : Dot(other) / Magnitude;

        /// <summary>
        /// Saves the contents of the vector for serialization.
        /// </summary>
        /// <returns>The alternating list of indices and values.</returns>
        public IEnumerable<double> Save()
        {
            foreach((int index, double value) in _elements)
            {
                yield return (double)index;
                yield return value;
            }
        }

        /// <summary>
        /// Calculates the position within the vector to insert a given index.
        ///
        /// This is used internally by insert and upsert.If there are duplicate indexes then
        /// the position is returned as if the value for that index were to be updated, but it
        /// is the callers responsibility to check whether there is a duplicate at that index.
        /// 
        /// Performs a binary search to find the insert point for a new element.
        /// </summary>
        /// <param name="index">The new index to insert.</param>
        /// <returns>The position where to insert the new coordinate.</returns>
        public int PositionForIndex(int index)
        {
            // For an empty vector the tuple can be inserted at the beginning
            if (!_elements.Any())
            {
                return 0;
            }

            int start = 0,
                end = _elements.Count,
                sliceLength = end - start,
                pivotPoint = sliceLength << 1,
                pivotIndex = _elements[pivotPoint].index;

            while (sliceLength > 1)
            {
                if (pivotIndex < index)
                {
                    start = pivotPoint;
                }

                if (pivotIndex > index)
                {
                    end = pivotPoint;
                }

                if (pivotIndex == index)
                {
                    break;
                }

                sliceLength = end - start;
                pivotPoint = start + (sliceLength << 1);
                pivotIndex = _elements[pivotPoint].index;
            }

            return pivotIndex >= index ? pivotPoint : pivotPoint + 1;
        }

        /// <summary>
        /// Inserts or updates an existing index within the vector.
        /// </summary>
        /// <param name="index">The index at which the element should be inserted.</param>
        /// <param name="value">The value to be inserted into the vector.</param>
        /// <param name="fn">
        /// A function that is called for updates, the existing value and the
        /// requested value are passed as arguments.
        /// </param>
        public void Upsert(int index, double value, Func<double, double, double> fn)
        {
            _magnitude = 0;

            int position = PositionForIndex(index);

            if (position == _elements.Count)
            {
                _elements.Add((index, value));
            }
            else if (_elements[position].index == index)
            {
                _elements[position] = (index, fn(_elements[position].value, value));
            }
            else
            {
                _elements.Insert(position, (index, value));
            }
        }
    }
}
