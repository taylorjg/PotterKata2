using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private static double TryToBeatBasePriceUsingDlx(IEnumerable<char> books, double basePrice)
        {
            var booksAndIndices = books.Select((book, index) => Tuple.Create(book, index)).ToList();
            var dlxData = BuildDlxData(booksAndIndices);
            var solutions = SolveDlx(dlxData, basePrice);
            var cheapestSolution = FindCheapestSolution(solutions, dlxData);
            return (cheapestSolution != null) ? CalculatePriceOfSolution(cheapestSolution, dlxData) : 0;
        }

        private static Solution FindCheapestSolution(IList<Solution> solutions, IList<Tuple<int[], string, double>> dlxData)
        {
            return solutions.Any() ? solutions.MinBy(solution => CalculatePriceOfSolution(solution, dlxData)) : null;
        }

        private static double CalculatePriceOfSolution(Solution solution, IList<Tuple<int[], string, double>> dlxData)
        {
            return solution.RowIndexes.Sum(rowIndex => dlxData[rowIndex].Item3);
        }

        private static List<Solution> SolveDlx(IList<Tuple<int[], string, double>> dlxData, double basePrice)
        {
            DumpDlxData(dlxData);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var dlx = new Dlx(cancellationToken);

            dlx.SolutionFound += (_, args) =>
                {
                    var solutionPrice = CalculatePriceOfSolution(args.Solution, dlxData);
                    if (solutionPrice < basePrice)
                    {
                        System.Diagnostics.Debug.WriteLine("Found a solution with a lower price than base price (solution index: {0})", args.SolutionIndex);
                        cancellationTokenSource.Cancel();
                    }
                };

            var solutions = dlx.Solve<
                IList<Tuple<int[], string, double>>,
                Tuple<int[], string, double>,
                int>(
                    dlxData,
                    (d, f) => { foreach (var r in d) f(r); },
                    (r, f) => { foreach (var c in r.Item1) f(c); },
                    c => c != 0).ToList();

            DumpDlxSolutions(dlxData, solutions);

            return solutions;
        }

        private static void DumpDlxData(ICollection<Tuple<int[], string, double>> dlxData)
        {
            foreach (var dlxDataRow in dlxData)
            {
                var indices = "[" + string.Join(",", dlxDataRow.Item1.Select(x => Convert.ToString(x))) + "]";
                System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}", indices, dlxDataRow.Item2, dlxDataRow.Item3);
            }

            System.Diagnostics.Debug.WriteLine("Number of rows in dlxData: {0}", dlxData.Count);
        }

        private static void DumpDlxSolutions(IList<Tuple<int[], string, double>> dlxData, ICollection<Solution> solutions)
        {
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            solutions.Select((solution, index) =>
                {
                    var solutionString = string.Join("|", solution.RowIndexes.Select(rowIndex => dlxData[rowIndex].Item2));
                    System.Diagnostics.Debug.WriteLine("solution[{0}]: {1}", index, solutionString);
                    return 0;
                });
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
            System.Diagnostics.Debug.WriteLine("Number of solutions: {0}", solutions.Count);
        }

        private static List<Tuple<int[], string, double>> BuildDlxData(IList<Tuple<char, int>> booksAndIndices)
        {
            var numColumns = booksAndIndices.Count;
            var dlxData = new List<Tuple<int[], string, double>>();
            BuildDlxDataIter(numColumns, dlxData, booksAndIndices);
            return dlxData;
        }

        private static void BuildDlxDataIter(
            int numColumns,
            ICollection<Tuple<int[], string, double>> dlxData,
            IList<Tuple<char, int>> booksAndIndices)
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

        private static void AddDlxDataRow(ICollection<Tuple<int[], string, double>> dlxData, Tuple<int[], string, double> dlxDataRow)
        {
            var existingRowWithSameIndices = dlxData.FirstOrDefault(x => x.Item1.SequenceEqual(dlxDataRow.Item1));
            if (existingRowWithSameIndices == null)
            {
                dlxData.Add(dlxDataRow);
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
