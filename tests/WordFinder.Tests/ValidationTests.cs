using WordFinderApi = global::WordFinder.WordFinder;

namespace WordFinder.Tests;

public class ValidationTests
{
    [Fact]
    public void Constructor_RejectsNullAndEmptyMatrices()
    {
        var nullMatrix = Assert.Throws<ArgumentNullException>(() => new WordFinderApi(null!));
        Assert.Equal("matrix", nullMatrix.ParamName);

        var emptyMatrix = Assert.Throws<ArgumentException>(() => new WordFinderApi([]));
        Assert.Equal("matrix", emptyMatrix.ParamName);
    }

    [Fact]
    public void Constructor_RejectsNullOrEmptyRowsAndUnequalLengths()
    {
        var nullRow = Assert.Throws<ArgumentNullException>(() => new WordFinderApi(new string[] { "ab", null! }));
        Assert.Equal("matrix", nullRow.ParamName);

        var emptyRow = Assert.Throws<ArgumentException>(() => new WordFinderApi(["ab", ""]));
        Assert.Equal("matrix", emptyRow.ParamName);

        var unequalRows = Assert.Throws<ArgumentException>(() => new WordFinderApi(["ab", "c"]));
        Assert.Equal("matrix", unequalRows.ParamName);
    }

    [Fact]
    public void Constructor_RejectsDimensionsAboveSixtyFour()
    {
        var tooManyRows = Assert.Throws<ArgumentException>(() => new WordFinderApi(Enumerable.Repeat("x", 65)));
        Assert.Equal("matrix", tooManyRows.ParamName);

        var tooManyColumns = Assert.Throws<ArgumentException>(() => new WordFinderApi([new string('x', 65)]));
        Assert.Equal("matrix", tooManyColumns.ParamName);
    }

    [Fact]
    public void Constructor_AcceptsOneByOneAndSixtyFourBySixtyFourMatrices()
    {
        var singleCell = new WordFinderApi(["x"]);
        Assert.Equal(["x"], singleCell.Find(["x"]));

        var maximumMatrix = new WordFinderApi(Enumerable.Repeat(new string('x', 64), 64));
        Assert.Equal(["x"], maximumMatrix.Find(["x"]));
    }

    [Fact]
    public void Constructor_MeasuresDimensionsInUtf16CodeUnitsAndAllowsUnpairedSurrogates()
    {
        var sixtyFourCodeUnits = string.Concat(Enumerable.Repeat("\uD83D\uDE00", 32));
        var valid = new WordFinderApi([sixtyFourCodeUnits]);
        Assert.Equal(["\uD83D"], valid.Find(["\uD83D"]));

        var sixtySixCodeUnits = string.Concat(Enumerable.Repeat("\uD83D\uDE00", 33));
        var tooWide = Assert.Throws<ArgumentException>(() => new WordFinderApi([sixtySixCodeUnits]));
        Assert.Equal("matrix", tooWide.ParamName);

        var unpaired = new WordFinderApi(["\uD800x"]);
        Assert.Equal(["\uD800"], unpaired.Find(["\uD800"]));
    }

    [Fact]
    public void Find_RejectsNullStreamAndNullOrEmptyWords()
    {
        var finder = new WordFinderApi(["abc"]);

        var nullStream = Assert.Throws<ArgumentNullException>(() => finder.Find(null!));
        Assert.Equal("wordstream", nullStream.ParamName);

        var nullWord = Assert.Throws<ArgumentNullException>(() => finder.Find(new string[] { "abc", null! }));
        Assert.Equal("wordstream", nullWord.ParamName);

        var emptyWord = Assert.Throws<ArgumentException>(() => finder.Find(["abc", ""]));
        Assert.Equal("wordstream", emptyWord.ParamName);
    }

    [Fact]
    public void Find_RejectsLateInvalidWordWithoutReturningPartialResults()
    {
        var finder = new WordFinderApi(["abc"]);

        var exception = Assert.Throws<ArgumentException>(() => finder.Find(["abc", "missing", ""]));

        Assert.Equal("wordstream", exception.ParamName);
    }

    [Fact]
    public void Find_AcceptsAnEmptyStream()
    {
        var finder = new WordFinderApi(["abc"]);

        Assert.Empty(finder.Find([]));
    }
}