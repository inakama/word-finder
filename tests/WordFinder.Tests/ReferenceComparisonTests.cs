using WordFinderApi = global::WordFinder.WordFinder;
using WordFinder.Workloads;

namespace WordFinder.Tests;

public class ReferenceComparisonTests
{
    [Fact]
    public void Find_MatchesIndependentReferenceForDeterministicHundredThousandCandidateWorkload()
    {
        var workload = DeterministicWorkload.Create();
        var repeatedWorkload = DeterministicWorkload.Create();

        Assert.Equal(64, workload.Matrix.Length);
        Assert.All(workload.Matrix, row => Assert.Equal(64, row.Length));
        Assert.Equal(DeterministicWorkload.CandidateCount, workload.Candidates.Length);
        Assert.Equal(workload.Fingerprint, repeatedWorkload.Fingerprint);
        Assert.Contains(workload.Candidates[0], workload.Candidates.Skip(1));

        var expected = FindByReference(workload.Matrix, workload.Candidates);
        var actual = new WordFinderApi(workload.Matrix).Find(workload.Candidates);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Find_MatchesIndependentReferenceForEverySupportedMatrixDimension()
    {
        for (var rowCount = 1; rowCount <= 64; rowCount++)
        {
            for (var columnCount = 1; columnCount <= 64; columnCount++)
            {
                var matrix = Enumerable.Range(0, rowCount)
                    .Select(rowIndex => new string(Enumerable.Range(0, columnCount)
                        .Select(columnIndex => (char)('a' + (rowIndex * 17 + columnIndex * 13) % 26))
                        .ToArray()))
                    .ToArray();
                var horizontalLength = Math.Min(4, columnCount);
                var horizontalStart = columnCount - horizontalLength;
                var horizontal = matrix[rowCount / 2].Substring(horizontalStart, horizontalLength);
                var verticalLength = Math.Min(4, rowCount);
                var verticalStart = rowCount - verticalLength;
                var vertical = new string(matrix.Select(row => row[columnCount / 2]).ToArray())
                    .Substring(verticalStart, verticalLength);
                var candidates = new[] { horizontal, vertical, matrix[0][0].ToString(), "!absent" };

                var expected = FindByReference(matrix, candidates);
                var actual = new WordFinderApi(matrix).Find(candidates);

                Assert.True(expected.SequenceEqual(actual), $"Reference mismatch for {rowCount}x{columnCount} matrix.");
            }
        }
    }

    private static string[] FindByReference(IEnumerable<string> matrix, IEnumerable<string> wordstream)
    {
        var rows = matrix.ToArray();
        var columns = Enumerable.Range(0, rows[0].Length)
            .Select(columnIndex => new string(rows.Select(row => row[columnIndex]).ToArray()))
            .ToArray();

        return wordstream
            .Distinct(StringComparer.Ordinal)
            .Select(word => (Word: word, Frequency: CountOccurrences(rows, word) + CountOccurrences(columns, word)))
            .Where(match => match.Frequency > 0)
            .OrderByDescending(match => match.Frequency)
            .ThenBy(match => match.Word, StringComparer.Ordinal)
            .Take(10)
            .Select(match => match.Word)
            .ToArray();
    }

    private static int CountOccurrences(string[] lines, string word)
    {
        var count = 0;
        foreach (var line in lines)
        {
            for (var startIndex = 0; startIndex <= line.Length - word.Length; startIndex++)
            {
                if (string.CompareOrdinal(line, startIndex, word, 0, word.Length) == 0)
                {
                    count++;
                }
            }
        }

        return count;
    }
}