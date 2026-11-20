# WordFinder

Searches a character matrix (up to 64x64) for words from an input stream. Words
can appear horizontally (left to right) or vertically (top to bottom). `Find`
returns up to 10 distinct words that appear in the matrix, ranked by their
number of occurrences. Repeating a word in the input stream does not affect its
frequency.

## Running the tests

Requires the .NET SDK 10.0.404 pinned in [global.json](global.json).

```powershell
dotnet test
```

The xUnit tests cover horizontal and vertical matches, boundaries, overlapping
occurrences, absent words, empty streams, repeated candidates, frequency
ordering, the ten-word limit, ordinal tie-breaking, input validation, UTF-16
code-unit matching, repeated searches, and concurrent calls.

Run the console example with:

```powershell
dotnet run --project examples/WordFinder.Demo
```

## How it works

The matrix is validated and captured in the constructor. Rows are retained and
the column strings are built once, so each search can examine contiguous lines
in both supported directions without changing or re-enumerating the input
matrix.

`Find` enumerates the word stream once, validates it, and de-duplicates
candidates with an ordinal `HashSet`. It then counts overlapping occurrences
of each distinct candidate across the rows and columns. Matches are ranked by
descending occurrence count, then by ascending ordinal word comparison, and
the first ten are returned.

The matrix is small and fixed in size, so each unique candidate is checked
against at most 64 rows and 64 columns, each no longer than 64 UTF-16 code
units. Total work still grows with the number and length of distinct candidates;
the candidate set is not capped. The per-call candidate set and ranking state
are local, while the captured matrix is not mutated, allowing concurrent calls
on the same instance (assuming the callers' input enumerables are themselves
safe to enumerate concurrently).

## Behavior and input contract

- Matching is case-sensitive and ordinal. Spaces are significant; strings are
  not trimmed or normalized.
- Only left-to-right and top-to-bottom matches count. Diagonal, reverse, and
  cross-row/cross-column matches do not.
- Each occurrence start position and direction counts, including overlapping
  matches. A word can occur in both directions and each occurrence contributes
  to its frequency.
- Equal-frequency matches are ordered using ordinal comparison; only the first
  ten words are returned.
- Matrix dimensions must be between 1x1 and 64x64, and all rows must have the
  same length. Lengths count UTF-16 code units.
- Null or empty matrices, null or empty rows, nonrectangular or oversized
  matrices, null streams, and null or empty words are rejected with argument
  exceptions.

## Performance

The benchmark project measures a deterministic 64x64 matrix and 100,000-word
workloads, including mostly-absent, duplicate-heavy, and many-match scenarios.
The reference comparison and benchmark share the same workload for the
mostly-absent case. Measurements are observational; the project defines no
numeric performance thresholds. The recorded environment and results are in
[research.md](specs/001-word-finder/research.md).

Run the benchmarks with:

```powershell
dotnet run -c Release --project benchmarks/WordFinder.Benchmarks
dotnet run -c Release --project benchmarks/WordFinder.Benchmarks -- --peak-memory
```

The selected scan approach avoids building an index of every possible
substring. A substring index or trie could change the trade-off between
construction cost, lookup work, and memory, but neither is implemented because
the current benchmark results do not establish that the additional complexity
is worthwhile. Ranking currently sorts all matching distinct words; a bounded
top-10 selection could be considered if measurements show ranking is a
bottleneck.

## Considerations for broader use

- `Find` stores each distinct input word until the search completes. Its memory
  use grows with the number and total size of unique candidates, so an input
  with an unbounded vocabulary may need a different API or workload-specific
  limits.
- The library's public C# API accepts `IEnumerable<string>` and is synchronous.
  A streaming asynchronous overload would be a separate library API design,
  not a replacement for the challenge contract.
- Case sensitivity and ordinal ordering are fixed by the documented contract;
  callers cannot select another comparison mode.

## Repository layout

| Path | Contents |
| --- | --- |
| `src/WordFinder/` | Core library |
| `tests/WordFinder.Tests/` | xUnit tests |
| `benchmarks/WordFinder.Benchmarks/` | BenchmarkDotNet benchmarks |
| `examples/WordFinder.Demo/` | Console example |
| `specs/001-word-finder/` | Specification, plan, tasks, and API contract |

See the [specification](specs/001-word-finder/spec.md),
[implementation plan](specs/001-word-finder/plan.md), and
[public C# API contract](specs/001-word-finder/contracts/public-api.md) for the
full design and requirements.
