using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DlxLib;

namespace Code
{
    public static class PotterBooks
    {
        public const double UnitBookPrice = 8d;

        private readonly static IDictionary<int, int> NumDifferentBooks2PercentDiscount = new Dictionary<int, int>
            {
                {0, 0},
                {1, 0},
                {2, 5},
                {3, 10},
                {4, 20},
                {5, 25}
            };

        internal static double CalculateSubTotalForSetOfBooks(IList<char> setOfBooks)
        {
            var numBooks = setOfBooks.Count();
            var numDistinctBooks = setOfBooks.Distinct().Count();
            var percentDiscount = NumDifferentBooks2PercentDiscount[numDistinctBooks];
            var subTotal = (numBooks * UnitBookPrice).PercentOff(percentDiscount);
            return subTotal;
        }

        private static double FindLowsetPriceUsingDlx(IList<char> books)
        {
            if (!books.Any())
            {
                return 0;
            }

            var booksAndIndices = books.Select((c, i) => Tuple.Create(c, i)).ToList();
            var numColumns = books.Count;
            var data = new List<Tuple<int[], string, double>>();

            for (;;)
            {
                var setOfBooks = FindBiggestDistinctSetOfBooks(books, booksAndIndices).ToList();

                var numDistinctBooks = setOfBooks.Count();
                if (numDistinctBooks == 0) break;
                if (numDistinctBooks == 1) setOfBooks = booksAndIndices.Where(_ => true).ToList();

                var dataRow = MakeDlxDataRow(numColumns, setOfBooks);
                data.Add(dataRow);
                foreach (var x in setOfBooks)
                {
                    books.Remove(x.Item1);
                    booksAndIndices.Remove(x);
                }
            }

            var dlx = new Dlx();
            var solutions = dlx.Solve<
                IList<Tuple<int[], string, double>>,
                Tuple<int[], string, double>,
                int>(
                    data,
                    (d, f) => { foreach (var r in d) f(r); },
                    (r, f) => { foreach (var c in r.Item1) f(c); },
                    c => c != 0);

            return solutions.Min(solution => solution.RowIndexes.Sum(rowIndex => data[rowIndex].Item3));
        }

        private static Tuple<int[], string, double> MakeDlxDataRow(int numColumns, IList<Tuple<char, int>> setOfBooks)
        {
            var columns = new int[numColumns];
            var indices = setOfBooks.Select(x => x.Item2);
            foreach (var index in indices)
            {
                columns[index] = 1;
            }
            var books = setOfBooks.Select(x => x.Item1).ToArray();
            var booksString = new string(books);
            var subTotal = CalculateSubTotalForSetOfBooks(books);
            return Tuple.Create(columns, booksString, subTotal);
        }

        private static IEnumerable<Tuple<char, int>> FindBiggestDistinctSetOfBooks(IEnumerable<char> books, IEnumerable<Tuple<char, int>> booksAndIndices)
        {
            var distinctBooks = books.Distinct().ToList();
            return distinctBooks.Select(book => booksAndIndices.First(x => x.Item1 == book)).ToList();
        }

        public static double CalculatePriceFor(string books)
        {
            return FindLowsetPriceUsingDlx(books.ToCharArray().ToList());
        }
    }
}
