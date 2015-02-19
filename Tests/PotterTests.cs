using Code;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class PotterTests
    {
        [Test]
        public void NoBooksCostZero()
        {
            var price = PotterBooks.CalculatePriceFor(string.Empty);
            AssertPrice(price, 0 * PotterBooks.UnitBookPrice);
        }

        [TestCase("A")]
        [TestCase("B")]
        [TestCase("C")]
        [TestCase("D")]
        [TestCase("E")]
        public void ASingleBookIsPricedCorrectly(string books)
        {
            var price = PotterBooks.CalculatePriceFor(books);
            AssertPrice(price, 1 * PotterBooks.UnitBookPrice);
        }

        [TestCase("AA", 2 * PotterBooks.UnitBookPrice)]
        [TestCase("AAA", 3 * PotterBooks.UnitBookPrice)]
        [TestCase("AAAA", 4 * PotterBooks.UnitBookPrice)]
        [TestCase("AAAAA", 5 * PotterBooks.UnitBookPrice)]
        [TestCase("BB", 2 * PotterBooks.UnitBookPrice)]
        [TestCase("BBB", 3 * PotterBooks.UnitBookPrice)]
        [TestCase("BBBB", 4 * PotterBooks.UnitBookPrice)]
        [TestCase("BBBBB", 5 * PotterBooks.UnitBookPrice)]
        [TestCase("AAAAAA", 6 * PotterBooks.UnitBookPrice)]
        [TestCase("AAAAAAA", 7 * PotterBooks.UnitBookPrice)]
        [TestCase("AAAAAAAA", 8 * PotterBooks.UnitBookPrice)]
        [TestCase("AAAAAAAAA", 9 * PotterBooks.UnitBookPrice)]
        [TestCase("AAAAAAAAAA", 10 * PotterBooks.UnitBookPrice)]
        public void MultipleBooksOfTheSameTypeArePricedCorrectly(string books, double expectedPrice)
        {
            var price = PotterBooks.CalculatePriceFor(books);
            AssertPrice(price, expectedPrice);
        }

        [TestCase("AB", 2 * PotterBooks.UnitBookPrice * 0.95d)]
        [TestCase("ABC", 3 * PotterBooks.UnitBookPrice * 0.90d)]
        [TestCase("CDE", 3 * PotterBooks.UnitBookPrice * 0.90d)]
        [TestCase("ACE", 3 * PotterBooks.UnitBookPrice * 0.90d)]
        [TestCase("ABCD", 4 * PotterBooks.UnitBookPrice * 0.80d)]
        [TestCase("BCDE", 4 * PotterBooks.UnitBookPrice * 0.80d)]
        [TestCase("ABCDE", 5 * PotterBooks.UnitBookPrice * 0.75d)]
        [TestCase("EDCBA", 5 * PotterBooks.UnitBookPrice * 0.75d)]
        public void MultipleBooksOfDifferentTypesArePricedCorrectly(string books, double expectedPrice)
        {
            var price = PotterBooks.CalculatePriceFor(books);
            AssertPrice(price, expectedPrice);
        }

        [TestCase("AAB", PotterBooks.UnitBookPrice + (2 * PotterBooks.UnitBookPrice * 0.95d))]
        [TestCase("AABB", 2 * (2 * PotterBooks.UnitBookPrice * 0.95d))]
        [TestCase("AABCCD", (4 * PotterBooks.UnitBookPrice * 0.80d) + (2 * PotterBooks.UnitBookPrice * 0.95d))]
        [TestCase("ABBCDE", PotterBooks.UnitBookPrice + (5 * PotterBooks.UnitBookPrice * 0.75d))]
        public void SimpleDiscountCombinationsArePricedCorrectly(string books, double expectedPrice)
        {
            var price = PotterBooks.CalculatePriceFor(books);
            AssertPrice(price, expectedPrice);
        }

        [TestCase("AABBCCDE", 2 * (4 * PotterBooks.UnitBookPrice * 0.80d))] // = ABCD + ABCE
        [TestCase("AAAAABBBBBCCCCDDDDDEEEE", (3 * (5 * PotterBooks.UnitBookPrice * 0.75d)) + (2 * (4 * PotterBooks.UnitBookPrice * 0.80d)))] // = ABCDE + ABCDE + ABCDE + ABCD + ABDE
        public void EdgeCaseIsPricedCorrectly(string books, double expectedPrice)
        {
            var price = PotterBooks.CalculatePriceFor(books);
            AssertPrice(price, expectedPrice);
        }

        private static void AssertPrice(double actualPrice, double expectedPrice)
        {
            Assert.That(actualPrice, Is.EqualTo(expectedPrice));
        }
    }
}
