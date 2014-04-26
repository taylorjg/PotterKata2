using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        internal static double CalculateSubTotalForSetOfDifferentBooks(IEnumerable<char> setOfBooks)
        {
            var setOfBooksAsList = setOfBooks.ToList();
            var numBooks = setOfBooksAsList.Count();
            var numDistinctBooks = setOfBooksAsList.Distinct().Count();
            Debug.Assert(numBooks > 1);
            Debug.Assert(numBooks == numDistinctBooks);
            var percentDiscount = NumDifferentBooks2PercentDiscount[numBooks];
            var subTotal = (numBooks * UnitBookPrice).PercentOff(percentDiscount);
            return subTotal;
        }

        internal static double CalculateSubTotalForSetOfSameBooks(IEnumerable<char> setOfBooks)
        {
            var setOfBooksAsList = setOfBooks.ToList();
            var numBooks = setOfBooksAsList.Count();
            var numDistinctBooks = setOfBooksAsList.Distinct().Count();
            Debug.Assert(numBooks > 0);
            Debug.Assert(numDistinctBooks == 1);
            var subTotal = (numBooks * UnitBookPrice);
            return subTotal;
        }

        private static double CalculatePriceByConsideringCombinations(IEnumerable<char> books)
        {
            var bookCalculations = new List<BookCalculation> { new BookCalculation(books) };

            for (; bookCalculations.Any(x => x.HasRemainingBooks);)
            {
                var newBookCalculations = new List<BookCalculation>();

                foreach (var bookCalculation in bookCalculations.Where(x => x.HasRemainingBooks))
                {
                    newBookCalculations.AddRange(bookCalculation.FindCombinationsOfNextSetOfBooks());
                }

                bookCalculations.RemoveAll(x => x.HasRemainingBooks);
                bookCalculations.AddRange(newBookCalculations);
            }

            var bookCalculationWithTheSmallestTotal = bookCalculations.MinBy(x => x.Total);
            return bookCalculationWithTheSmallestTotal.Total;
        }

        public static double CalculatePriceFor(string books)
        {
            return CalculatePriceByConsideringCombinations(books.ToCharArray());
        }
    }
}
