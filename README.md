# Word Finder

A .NET solution for the **QuBeyond Developer Challenge: Word Finder**. Given a character matrix, it finds which words from an input stream appear in it, searching only horizontally (left to right) and vertically (top to bottom), and returns at most the ten most frequent matches.

## Requirements

- [.NET SDK 10.0.404](global.json) (pinned in `global.json`)

## Usage

```csharp
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

IEnumerable<string> found = wordFinder.Find(wordstream);
// chill, cold, wind
```

The public API:

```csharp
public class WordFinder
{
    public WordFinder(IEnumerable<string> matrix);
    public IEnumerable<string> Find(IEnumerable<string> wordstream);
}
```

## Search rules

- Words are matched horizontally (left to right) and vertically (top to bottom) only. Diagonal and reverse matches are not counted.
- Each qualifying start position and direction counts as one occurrence, including overlapping matches.
- A word repeated in the input stream is counted once.
- Results are ordered by frequency (descending), then by ordinal comparison of the word.
- At most 10 words are returned. Absent words are omitted; if nothing matches, the result is empty.
- Matrix limits: 1 to 64 rows, 1 to 64 columns, all rows the same length. Characters are counted as UTF-16 code units.
- Invalid input throws: a null or empty matrix, a nonrectangular matrix, a row longer than 64 or empty, a null or empty word in the stream, or a null stream.
- A single instance supports concurrent `Find` calls.

## Build and test

```powershell
dotnet build
dotnet test
```

## Demo

```powershell
dotnet run --project examples/WordFinder.Demo
```

## Benchmarks

```powershell
dotnet run -c Release --project benchmarks/WordFinder.Benchmarks
dotnet run -c Release --project benchmarks/WordFinder.Benchmarks -- --peak-memory
```

Benchmarks record elapsed time, allocations, and peak working set for a 100,000-candidate workload. There are no pass/fail limits. Results are in [research.md](specs/001-word-finder/research.md) and `BenchmarkDotNet.Artifacts/`.

## Repository layout

| Path | Contents |
| --- | --- |
| `src/WordFinder/` | Library implementation (`WordFinder.cs`) |
| `tests/WordFinder.Tests/` | xUnit tests |
| `benchmarks/WordFinder.Benchmarks/` | BenchmarkDotNet benchmarks |
| `examples/WordFinder.Demo/` | Console demo |
| `specs/001-word-finder/` | Specification, plan, tasks, and contracts |

## Documentation

- [Specification](specs/001-word-finder/spec.md)
- [Implementation plan](specs/001-word-finder/plan.md)
- [Public API contract](specs/001-word-finder/contracts/public-api.md)
- [Quickstart](specs/001-word-finder/quickstart.md)
