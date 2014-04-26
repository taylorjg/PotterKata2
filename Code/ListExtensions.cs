using System.Collections.Generic;

namespace Code
{
    public static class ListExtensions
    {
        public static void RemoveRange<TSource>(this IList<TSource> source, IEnumerable<TSource> collection)
        {
            foreach (var item in collection) source.Remove(item);
        }

        public static IList<TSource> Copy<TSource>(this IList<TSource> source)
        {
            return new List<TSource>(source);
        }

        public static IList<TSource> CopyExcept<TSource>(this IList<TSource> source, IEnumerable<TSource> collection)
        {
            var copyOfList = source.Copy();
            copyOfList.RemoveRange(collection);
            return copyOfList;
        }
    }
}
