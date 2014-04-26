using System;
using System.Collections.Generic;
using System.Linq;

namespace Code
{
    public static class EnumerableExtensionsKnickedFromStackOverflow
    {
        // http://stackoverflow.com/questions/5980810/generate-all-unique-combinations-of-elements-of-a-ienumerableof-t
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                from accseq in accumulator
                // Exclude items that were already picked
                from item in sequence.Except(accseq)
                // Enforce ascending order to avoid same sequence in different order
                where !accseq.Any() || Comparer<T>.Default.Compare(item, accseq.Last()) > 0
                select accseq.Concat(new[] { item })).ToArray();
        }

        // http://stackoverflow.com/questions/914109/how-to-use-linq-to-select-object-with-minimum-or-maximum-property-value
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, Comparer<TKey>.Default);
        }

        // http://stackoverflow.com/questions/914109/how-to-use-linq-to-select-object-with-minimum-or-maximum-property-value
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            //source.ThrowIfNull("source");
            //selector.ThrowIfNull("selector");
            //comparer.ThrowIfNull("comparer");
            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence was empty");
                }
                TSource min = sourceIterator.Current;
                TKey minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    TSource candidate = sourceIterator.Current;
                    TKey candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }
    }
}
