## Description

This is an attempt at the Potter kata using the Donald E. Knuth's Dancing Links (DLX) algorithm.

## Exact Cover Problem

The idea is to build a matrix where each row identifies a subset of the books and the
corresponding subtotal after the discount has been applied.
Then, use DLX to find all the solutions to the matrix where each solution identifies a
set of rows that together form the original full set of books. Then, we just need to find
the solution that has the cheapest total (where the total is the sum of the subtotals of the rows).

If the original set of books is <code>AABBCCDE</code>, then I assign an index to each character to give:

```
AABBCCDE
01234567
```

I build an internal structure that consists of rows with the following format:

* An array of 0s/1s that identifies exactly which books in the original set of books have been used to make this subset of books
* A string value representing the subset of books (for convenience - not really used)
* A double value that is the price of the subset of books after applying the appropriate discount

So given the original set of books above, a couple of rows might look as follows (these two rows, taken
together, do in fact describe the cheapest solution):

```
01234567  subset of books   price
---------------------------------
... other rows ...
10101010  "ABCD"  25.6
... other rows ...
01010101  "ABCE"  25.6
... other rows ...
```

Only the array of ints is used by DLX when finding solutions. It is looking for combinations of rows such that each column
contains exactly one 1:

```
10101010  ; this set of books...
01010101  ; ...and this set of books...
11111111  ; ...combine to give this overall set of books
```

i.e. every book has been used exactly once - we have exactly covered the original set of books.

## How It Turned Out

Well, it does work but it is slow. Why is it slow? I imagined that it was because the input matrix had loads
of rows. But when I investigated, the matrix was not very big. However, it did have millions of solutions!
To counteract this, I now calculate a base price using the more obvious algorithm of
"keep applying the biggest discount". I then use the DLX approach above but I stop trying to find solutions
as soon as I find one that is cheaper than the base price. This might be good enough. The unit tests pass anyway!

## Links

* http://codingdojo.org/cgi-bin/index.pl?KataPotter
* [DlxLib](https://github.com/taylorjg/DlxLib "DlxLib")
