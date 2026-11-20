using WordFinder;

var matrix = new[]
{
    "abcdc",
    "fgwio",
    "chill",
    "pqnsd",
    "uvdxy"
};

var wordFinder = new WordFinder.WordFinder(matrix);

var wordstream = new[] { "cold", "wind", "snow", "chill", "wind", "chill" };

var found = wordFinder.Find(wordstream);

Console.WriteLine("Words found (top 10 by frequency): " + string.Join(", ", found));
