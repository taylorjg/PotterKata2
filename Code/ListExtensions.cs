using System.Collections.Generic;

namespace Code
{
    public static class ListExtensions
    {
        public static void RemoveRange<TSource>(this IList<TSource> source, IEnumerable<TSource> collection)
        {
            foreach (var item in collection) source.Remove(item);
        }
    }
}
