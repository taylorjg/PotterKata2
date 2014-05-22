using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;

namespace Code
{
    using BookAndIndex = Tuple<char, int>;
    using DlxDataRow = Tuple<int[], string, double>;

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

        private static double TryToBeatBasePriceUsingDlx(IEnumerable<char> books, double basePrice)
        {
            var booksAndIndices = books.Select((book, index) => Tuple.Create(book, index)).ToList();
            var dlxData = BuildDlxData(booksAndIndices);
            var cheapestSolution = SolveDlx(dlxData, basePrice);
            return (cheapestSolution != null) ? cheapestSolution.Item2 : basePrice;
        }

        private static Tuple<Solution, double> SolveDlx(IList<DlxDataRow> dlxData, double basePrice)
        {
            DumpDlxData(dlxData);

            var solutions = new Dlx().Solve<IList<DlxDataRow>, DlxDataRow, int>(
                dlxData,
                (d, f) => { foreach (var r in d) f(r); },
                (r, f) => { foreach (var c in r.Item1) f(c); },
                c => c != 0);

            return solutions
                .Select(solution => Tuple.Create(solution, CalculatePriceOfSolution(solution, dlxData)))
                .FirstOrDefault(x => x.Item2 < basePrice);
        }

        private static double CalculatePriceOfSolution(Solution solution, IList<DlxDataRow> dlxData)
        {
            return solution.RowIndexes.Sum(rowIndex => dlxData[rowIndex].Item3);
        }

        private static void DumpDlxData(ICollection<DlxDataRow> dlxData)
        {
            foreach (var dlxDataRow in dlxData)
            {
                var indices = "[" + string.Join(",", dlxDataRow.Item1.Select(x => Convert.ToString(x))) + "]";
                System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}", indices, dlxDataRow.Item2, dlxDataRow.Item3);
            }

            System.Diagnostics.Debug.WriteLine("Number of rows in dlxData: {0}", dlxData.Count);
        }

        private static List<DlxDataRow> BuildDlxData(IList<BookAndIndex> booksAndIndices)
        {
            var numColumns = booksAndIndices.Count;
            var dlxData = new List<DlxDataRow>();
            BuildDlxDataIter(numColumns, dlxData, booksAndIndices);
            return dlxData;
        }

        private static void BuildDlxDataIter(
            int numColumns,
            ICollection<DlxDataRow> dlxData,
            IList<BookAndIndex> booksAndIndices)
        {
            var combinations = GetNextSetOfBookCombinations(booksAndIndices).ToList();

            if (combinations.Any())
            {
                // Optimisation - if we have any combinations consisting of 4 or more books
                // then don't bother considering smaller combinations.
                if (combinations.Any(x => x.Count() >= 4))
                {
                    combinations = combinations.Where(x => x.Count() >= 4).ToList();
                }

                foreach (var combination in combinations.Select(x => x.ToList()))
                {
                    AddDlxDataRow(dlxData, MakeDlxDataRow(numColumns, combination));
                    var remainingBooksAndIndices = booksAndIndices.CopyExcept(combination);
                    BuildDlxDataIter(numColumns, dlxData, remainingBooksAndIndices);
                }
            }
            else
            {
                if (booksAndIndices.Any())
                {
                    AddDlxDataRow(dlxData, MakeDlxDataRow(numColumns, booksAndIndices));
                }
            }
        }

        private static void AddDlxDataRow(ICollection<DlxDataRow> dlxData, DlxDataRow dlxDataRow)
        {
            var existingRowWithSameIndices = dlxData.FirstOrDefault(x => x.Item1.SequenceEqual(dlxDataRow.Item1));
            if (existingRowWithSameIndices == null)
            {
                dlxData.Add(dlxDataRow);
            }
        }

        private static IEnumerable<IEnumerable<BookAndIndex>> GetNextSetOfBookCombinations(IList<BookAndIndex> booksAndIndices)
        {
            var remainingBooks = booksAndIndices.Select(x => x.Item1);
            var seed = Enumerable.Empty<IEnumerable<char>>();

            return Enumerable.Range(2, 5)
                             .Aggregate(
                                 seed,
                                 (acc, i) => acc.Concat(
                                     Enumerable.Repeat(remainingBooks, i)
                                               .Combinations()))
                             .Select(cs => cs.Select(c => booksAndIndices.First(x => x.Item1 == c)));
        }

        private static DlxDataRow MakeDlxDataRow(int numColumns, IList<BookAndIndex> setOfBooks)
        {
            var columns = new int[numColumns];
            var indices = setOfBooks.Select(x => x.Item2);
            foreach (var index in indices) columns[index] = 1;
            var books = setOfBooks.Select(x => x.Item1).ToArray();
            var booksAsAString = new string(books);
            var subTotal = CalculateSubTotalForSetOfBooks(books);
            return Tuple.Create(columns, booksAsAString, subTotal);
        }

        private static double FindBasePrice(IList<char> books)
        {
            return FindBasePriceIter(books, 0d);
        }

        private static double FindBasePriceIter(IList<char> remainingBooks, double totalSoFar)
        {
            var setOfBooks = remainingBooks.Distinct().ToList();
            if (!setOfBooks.Any()) return totalSoFar;
            var newTotalSoFar = totalSoFar + CalculateSubTotalForSetOfBooks(setOfBooks);
            var newRemainingBooks = remainingBooks.CopyExcept(setOfBooks);
            return FindBasePriceIter(newRemainingBooks, newTotalSoFar);
        }

        public static double CalculatePriceFor(string books)
        {
            var booksAsArray = books.ToCharArray();
            var basePrice = FindBasePrice(booksAsArray);
            return TryToBeatBasePriceUsingDlx(booksAsArray, basePrice);
        }
    }
}
