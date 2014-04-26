using System;
using System.Collections.Generic;
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

        private static double FindLowsetPriceUsingDlx(IEnumerable<char> books)
        {
            var booksAndIndices = books.Select((book, index) => Tuple.Create(book, index)).ToList();
            var dlxData = BuildDlxData(booksAndIndices);
            var solutions = SolveDlx(dlxData);
            return FindDlxSolutionWithLowestPrice(solutions, dlxData);
        }

        private static double FindDlxSolutionWithLowestPrice(IList<Solution> solutions, IList<Tuple<int[], string, double>> dlxData)
        {
            return !solutions.Any() ? 0 : solutions.Min(solution => CalculatePriceOfSolution(solution, dlxData));
        }

        private static double CalculatePriceOfSolution(Solution solution, IList<Tuple<int[], string, double>> dlxData)
        {
            return solution.RowIndexes.Sum(rowIndex => dlxData[rowIndex].Item3);
        }

        private static List<Solution> SolveDlx(IList<Tuple<int[], string, double>> dlxData)
        {
            var dlx = new Dlx();
            var solutions = dlx.Solve<
                IList<Tuple<int[], string, double>>,
                Tuple<int[], string, double>,
                int>(
                    dlxData,
                    (d, f) => { foreach (var r in d) f(r); },
                    (r, f) => { foreach (var c in r.Item1) f(c); },
                    c => c != 0).ToList();
            return solutions;
        }

        private static List<Tuple<int[], string, double>> BuildDlxData(IList<Tuple<char, int>> booksAndIndices)
        {
            var numColumns = booksAndIndices.Count;
            var dlxData = new List<Tuple<int[], string, double>>();
            Iter(numColumns, dlxData, booksAndIndices);
            return dlxData;
        }

        private static void Iter(int numColumns, ICollection<Tuple<int[], string, double>> dlxData, IList<Tuple<char, int>> booksAndIndices)
        {
            var combinations = GetNextSetOfBookCombinations(booksAndIndices).ToList();

            if (combinations.Any())
            {
                foreach (var combination in combinations.Select(x => x.ToList()))
                {
                    dlxData.Add(MakeDlxDataRow(numColumns, combination));
                    var remainingBooksAndIndices = booksAndIndices.CopyExcept(combination);
                    Iter(numColumns, dlxData, remainingBooksAndIndices);
                }
            }
            else
            {
                dlxData.Add(MakeDlxDataRow(numColumns, booksAndIndices));
            }
        }

        private static IEnumerable<IEnumerable<Tuple<char, int>>> GetNextSetOfBookCombinations(IList<Tuple<char, int>> booksAndIndices)
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

        private static Tuple<int[], string, double> MakeDlxDataRow(int numColumns, IList<Tuple<char, int>> setOfBooks)
        {
            var columns = new int[numColumns];
            var indices = setOfBooks.Select(x => x.Item2);
            foreach (var index in indices) columns[index] = 1;
            var books = setOfBooks.Select(x => x.Item1).ToArray();
            var booksAsAString = new string(books);
            var subTotal = CalculateSubTotalForSetOfBooks(books);
            return Tuple.Create(columns, booksAsAString, subTotal);
        }

        public static double CalculatePriceFor(string books)
        {
            return FindLowsetPriceUsingDlx(books.ToCharArray());
        }
    }
}
