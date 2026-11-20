namespace WordFinder;

public class WordFinder
{
    private readonly string[] rows;
    private readonly string[] columns;

    public WordFinder(IEnumerable<string> matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        var capturedRows = new List<string>();
        var columnCount = -1;
        foreach (var row in matrix)
        {
            if (capturedRows.Count == 64)
            {
                throw new ArgumentException("The matrix cannot contain more than 64 rows.", nameof(matrix));
            }

            if (row is null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            if (row.Length == 0)
            {
                throw new ArgumentException("Matrix rows cannot be empty.", nameof(matrix));
            }

            if (row.Length > 64)
            {
                throw new ArgumentException("Matrix rows cannot exceed 64 UTF-16 code units.", nameof(matrix));
            }

            if (columnCount < 0)
            {
                columnCount = row.Length;
            }
            else if (row.Length != columnCount)
            {
                throw new ArgumentException("All matrix rows must have the same length.", nameof(matrix));
            }

            capturedRows.Add(row);
        }

        if (capturedRows.Count == 0)
        {
            throw new ArgumentException("The matrix cannot be empty.", nameof(matrix));
        }

        rows = capturedRows.ToArray();
        columns = new string[columnCount];

        for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
        {
            var column = new char[rows.Length];
            for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                column[rowIndex] = rows[rowIndex][columnIndex];
            }

            columns[columnIndex] = new string(column);
        }
    }

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
        ArgumentNullException.ThrowIfNull(wordstream);

        var candidates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var word in wordstream)
        {
            if (word is null)
            {
                throw new ArgumentNullException(nameof(wordstream));
            }

            if (word.Length == 0)
            {
                throw new ArgumentException("Words cannot be empty.", nameof(wordstream));
            }

            candidates.Add(word);
        }

        var matches = new List<(string Word, int Frequency)>();
        var longestLineLength = Math.Max(rows[0].Length, columns[0].Length);

        foreach (var word in candidates)
        {
            if (word.Length <= longestLineLength)
            {
                var frequency = CountOccurrences(rows, word) + CountOccurrences(columns, word);
                if (frequency > 0)
                {
                    matches.Add((word, frequency));
                }
            }
        }

        return matches
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
            if (line.Length < word.Length)
            {
                continue;
            }

            var searchStartIndex = 0;
            while (searchStartIndex <= line.Length - word.Length)
            {
                var matchIndex = line.AsSpan(searchStartIndex).IndexOf(word.AsSpan(), StringComparison.Ordinal);
                if (matchIndex < 0)
                {
                    break;
                }

                count++;
                searchStartIndex += matchIndex + 1;
            }
        }

        return count;
    }
}