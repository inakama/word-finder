using System.Collections;
using WordFinderApi = global::WordFinder.WordFinder;

namespace WordFinder.Tests;

public class FindTests
{
    [Fact]
    public void Find_ReturnsHorizontalAndVerticalMatchesOnly()
    {
        var finder = new WordFinderApi(["abcd", "efgh", "ijkl", "mnop"]);

        var result = finder.Find(["abc", "bfj", "afk", "cba", "def", "xxxx"]);

        Assert.Equal(["abc", "bfj"], result);
    }

    [Fact]
    public void Find_ReturnsEmptyForEmptyOrUnmatchedStreams()
    {
        var finder = new WordFinderApi(["abc", "def", "ghi"]);

        Assert.Empty(finder.Find([]));
        Assert.Empty(finder.Find(["xyz", "nope"]));
    }

    [Fact]
    public void Find_IncludesFirstAndLastBoundariesWithoutWrapping()
    {
        var finder = new WordFinderApi(["abcd", "efgh", "ijkl", "mnop"]);

        var result = finder.Find(["ab", "cd", "ae", "mn", "de", "gh"]);

        Assert.Equal(["ab", "ae", "cd", "gh", "mn"], result);
    }

    [Fact]
    public void Find_UsesExactCaseSensitiveUntrimmedUtf16Matching()
    {
        var finder = new WordFinderApi(["a b", "Axy", "e\u0301z"]);

        var result = finder.Find(["a ", "A", "é", "e\u0301", "a b"]);

        Assert.Equal(["A", "a ", "a b", "e\u0301"], result);
    }

    [Fact]
    public void Find_MatchesSupplementaryAndIndividualSurrogateCodeUnitsInBothDirections()
    {
        var high = "\uD83D";
        var low = "\uDE00";
        var smile = high + low;
        var finder = new WordFinderApi([smile + "x", "a" + high + "b"]);

        var result = finder.Find([smile, high, low, high + "b", "x", "a" + high]);

        Assert.Equal([high, "x", low, "a" + high, high + "b", smile], result);
    }

    [Fact]
    public void Find_RanksOverlappingOccurrencesAcrossBothDirections()
    {
        var finder = new WordFinderApi(["aaa", "aaa"]);

        var result = finder.Find(["aa", "a", "aaa"]);

        Assert.Equal(["a", "aa", "aaa"], result);
    }

    [Fact]
    public void Find_CountsSingleCodeUnitHitsInBothDirections()
    {
        var finder = new WordFinderApi(["ab", "ab"]);

        var result = finder.Find(["ab", "b"]);

        Assert.Equal(["b", "ab"], result);
    }

    [Fact]
    public void Find_DuplicateCandidatesDoNotChangeFrequencyOrOrdering()
    {
        var finder = new WordFinderApi(["abba"]);

        var result = finder.Find(["ba", "ab", "ba", "ab", "ba"]);

        Assert.Equal(["ab", "ba"], result);
    }

    [Fact]
    public void Find_UsesOrdinalTieBreakingAndReturnsAtMostTenWords()
    {
        var finder = new WordFinderApi(["abcdefghijkl"]);

        var result = finder.Find(["l", "k", "j", "i", "h", "g", "f", "e", "d", "c", "b", "a"]);

        Assert.Equal(["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"], result);

        var caseSensitiveTies = new WordFinderApi(["aA"]).Find(["a", "A"]);
        Assert.Equal(["A", "a"], caseSensitiveTies);
    }

    [Fact]
    public void Find_MatchesUnpairedSurrogateCodeUnits()
    {
        const string unpairedHighSurrogate = "\uD800";
        var finder = new WordFinderApi([unpairedHighSurrogate + "x", unpairedHighSurrogate + "y"]);

        var result = finder.Find([unpairedHighSurrogate + "y", unpairedHighSurrogate, unpairedHighSurrogate + "x"]);

        Assert.Equal([unpairedHighSurrogate, unpairedHighSurrogate + "x", unpairedHighSurrogate + "y"], result);
    }

    [Fact]
    public void Find_CapturesMatrixAndSupportsRepeatedCallsAndSingleEnumeration()
    {
        var rows = new List<string> { "abc", "def", "ghi" };
        var finder = new WordFinderApi(rows);
        rows[0] = "xxx";
        var stream = new CountingEnumerable("abc", "abc", "adg");

        Assert.Equal(["abc", "adg"], finder.Find(stream));
        Assert.Equal(1, stream.EnumerationCount);
        Assert.Equal(["abc"], finder.Find(["abc"]));
    }

    [Fact]
    public void Find_SearchesTheDirectionThatCanFitTheWord()
    {
        var finder = new WordFinderApi(["abcd", "efgh"]);

        var result = finder.Find(["abcd", "ae", "bf"]);

        Assert.Equal(["abcd", "ae", "bf"], result);
    }

    [Fact]
    public void Find_MatchesQuickstartExample()
    {
        var finder = new WordFinderApi(["abcdc", "fgwio", "chill", "pqnsd", "uvdxy"]);

        var result = finder.Find(["cold", "wind", "snow", "chill"]);

        Assert.Equal(["chill", "cold", "wind"], result);
    }

    [Fact]
    public async Task Find_ConcurrentCallsHaveIndependentResults()
    {
        var finder = new WordFinderApi(["abcd", "efgh", "ijkl", "mnop"]);
        using var barrier = new Barrier(2);

        var first = Task.Run(() =>
        {
            barrier.SignalAndWait();
            return finder.Find(["abc", "bfj", "xxxx"]).ToArray();
        });
        var second = Task.Run(() =>
        {
            barrier.SignalAndWait();
            return finder.Find(["cde", "gk", "mnop"]).ToArray();
        });

        var results = await Task.WhenAll(first, second);

        Assert.Equal(["abc", "bfj"], results[0]);
        Assert.Equal(["gk", "mnop"], results[1]);
    }

    private sealed class CountingEnumerable(params string[] values) : IEnumerable<string>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<string> GetEnumerator()
        {
            EnumerationCount++;
            return ((IEnumerable<string>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}