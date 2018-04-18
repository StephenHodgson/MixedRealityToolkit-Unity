﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Extensions
{
    /// <summary>
    /// Extension methods for .Net Collection objects, e.g. Lists, Dictionarys, Arrays
    /// </summary>
    public static class CollectionsExtensions
    {
        /// <summary>
        /// Creates a read-only wrapper around an existing collection.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the collection.</typeparam>
        /// <param name="elements">The collection to be wrapped.</param>
        /// <returns>The new, read-only wrapper around <paramref name="elements"/>.</returns>
        public static ReadOnlyCollection<TElement> AsReadOnly<TElement>(this IList<TElement> elements)
        {
            return new ReadOnlyCollection<TElement>(elements);
        }

        /// <summary>
        /// Creates a read-only copy of an existing collection.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the collection.</typeparam>
        /// <param name="elements">The collection to be copied.</param>
        /// <returns>The new, read-only copy of <paramref name="elements"/>.</returns>
        public static ReadOnlyCollection<TElement> ToReadOnlyCollection<TElement>(this IEnumerable<TElement> elements)
        {
            return elements.ToArray().AsReadOnly();
        }

        /// <summary>
        /// Inserts an item in its sorted position into an already sorted collection. This is useful if you need to consume the
        /// collection in between insertions and need it to stay correctly sorted the whole time. If you just need to insert a
        /// bunch of items and then consume the sorted collection at the end, it's faster to add all the elements and then use
        /// <see cref="List{T}.Sort"/> at the end.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the collection.</typeparam>
        /// <param name="elements">The collection of sorted elements to be inserted into.</param>
        /// <param name="toInsert">The element to insert.</param>
        /// <param name="comparer">The comparer to use when sorting or <see cref="null"/> to use <see cref="Comparer{T}.Default"/>.</param>
        /// <returns></returns>
        public static int SortedInsert<TElement>(this List<TElement> elements, TElement toInsert, IComparer<TElement> comparer = null)
        {
            var effectiveComparer = comparer ?? Comparer<TElement>.Default;

#if DEBUG || UNITY_EDITOR
            for (int iElement = 0; iElement < elements.Count - 1; iElement++)
            {
                var element = elements[iElement];
                var nextElement = elements[iElement + 1];

                if (effectiveComparer.Compare(element, nextElement) > 0)
                {
                     Debug.Assert(false, "elements must already be sorted to call this method.");
                    break;
                }
            }
#endif

            int searchResult = elements.BinarySearch(toInsert, effectiveComparer);

            int insertionIndex = searchResult >= 0
                ? searchResult
                : ~searchResult;

            elements.Insert(insertionIndex, toInsert);

            return insertionIndex;
        }

        /// <summary>
        /// Disposes of all non-null elements in a collection.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the collection.</typeparam>
        /// <param name="elements">The collection of elements to be disposed.</param>
        public static void DisposeElements<TElement>(this IEnumerable<TElement> elements)
            where TElement : IDisposable
        {
            foreach (var element in elements)
            {
                if (element != null)
                {
                    element.Dispose();
                }
            }
        }

        /// <summary>
        /// Disposes of all non-null elements in a collection.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the collection.</typeparam>
        /// <param name="elements">The collection of elements to be disposed.</param>
        public static void DisposeElements<TElement>(this IList<TElement> elements)
            where TElement : IDisposable
        {
            for (int iElement = 0; iElement < elements.Count; iElement++)
            {
                var element = elements[iElement];

                if (element != null)
                {
                    element.Dispose();
                }
            }
        }

    }
}
