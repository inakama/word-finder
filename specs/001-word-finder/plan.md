# Implementation Plan: WordFinder

**Branch**: `001-word-finder` | **Date**: 2026-11-19 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-word-finder/spec.md`

## Summary

Implement `WordFinder` as a small .NET class library. The constructor validates and captures the matrix (rows plus precomputed column strings). `Find` validates and de-duplicates the word stream in a single enumeration, counts overlapping ordinal UTF-16 code-unit occurrences of each distinct word in rows (left to right) and columns (top to bottom), and returns the top ten by descending frequency with ascending ordinal tie-breaking. Per-call state is isolated for concurrent searches. Validate a shared 100,000-candidate workload against a reference and measure its resource use (SC-005).

## Technical Context

**Language/Version**: C# 14 on .NET 10 (SDK pinned by `global.json`, 10.0.404)

**Primary Dependencies**: None for the library. Test project: xUnit. Benchmark project: BenchmarkDotNet.

**Storage**: N/A (in-memory)

**Testing**: xUnit, run with `dotnet test`

**Target Platform**: Cross-platform .NET 10 class library

**Project Type**: Library (plus test and benchmark projects)

**Performance Goals**: No numeric pass/fail limits. Record elapsed time and peak memory for a 64x64 matrix and 100,000 candidates, with the workload and environment.

**Constraints**: Matrix 1-64 rows x 1-64 UTF-16 code-unit cells; exact, case-sensitive, ordinal matching including unpaired surrogates; independent concurrent searches; at most 10 results; no partial results on invalid input; public contract unchanged.

**Scale/Scope**: ≤ 4,096 matrix cells; word streams of 100,000+ entries; one public type.

## Constitution Check

*GATE: design passes before Phase 0 and after Phase 1. Measurement and tests remain implementation-time obligations, not completed results.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Code quality / minimal public API | Pass | One public class, two members. Everything else `internal`/`private`. |
| Library-first, encapsulation, separation | Pass | `src/`, `tests/`, `benchmarks/` are separate projects. |
| No unnecessary dependencies/infrastructure | Pass | Only xUnit and BenchmarkDotNet, in non-production projects. |
| Performance validated by benchmarks | Pass | Planned measurements use a reference-verified 100k workload; unmeasured speed assumptions are not presented as findings. |
| Testing (deterministic, covers FR and edge cases) | Pass | Planned coverage includes UTF-16 and concurrent calls; see Testing Strategy. |
| Spec-driven, documented decisions | Pass | See [research.md](./research.md). |
| Public interface preserved | Pass | See [contracts/public-api.md](./contracts/public-api.md). |

No violations; Complexity Tracking is not needed.

## Design Decisions (summary; rationale in research.md)

1. **Matrix representation**: validate, then keep `string[] rows` and `string[] columns` (columns built once in the constructor, O(R·C)). Copy the row collection; strings are immutable. Index columns by UTF-16 code unit, including individual surrogate halves, and never normalize or reject unpaired surrogates.
2. **Search algorithm**: for each distinct word, count overlapping occurrences in every row and column with `ReadOnlySpan<char>.IndexOf(word, StringComparison.Ordinal)`, restarting at `index + 1` so overlaps count (FR-007). Cheap pre-filters: skip words longer than `max(rows, columns)`; skip a direction whose line length is shorter than the word.
3. **No Trie / substring index** initially: a full substring index considers up to ~266k start/length pairs (128 lines × 64 × 65 / 2) and requires additional construction work and storage. Consider it only if measured workloads justify the complexity; make no unmeasured speed or allocation comparison.
4. **Stream handling**: enumerate once into a `HashSet<string>(StringComparer.Ordinal)`, validating each word (null/empty throws) before any searching, so invalid input yields no partial result (FR-010, FR-011).
5. **Ranking**: collect `(word, count)` for count > 0, order by count descending then `string.CompareOrdinal` ascending, take 10. A bounded size-10 heap is a possible refinement if benchmarks show sorting matters.
6. **Validation errors**: `ArgumentNullException` for null inputs or elements; `ArgumentException` (with `paramName`) for empty rows/words, unequal row lengths, or dimensions above 64.
7. **Thread safety**: matrix state is immutable after construction; each `Find` call owns its validation, deduplication, counting, and result state. Concurrent calls on the same instance return independent results (FR-001). Callers are responsible for the safety of any input enumerable shared across calls.

## Project Structure

### Documentation (this feature)

```text
specs/001-word-finder/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── public-api.md
└── tasks.md             # refresh after planning changes
```

### Source Code (repository root)

```text
WordFinder.slnx
global.json
src/
└── WordFinder/
    ├── WordFinder.csproj
    └── WordFinder.cs
tests/
└── WordFinder.Tests/
    ├── WordFinder.Tests.csproj
    ├── FindTests.cs                 # directions, frequency, ranking, limits
    ├── ValidationTests.cs           # constructor and stream validation
    └── ReferenceComparisonTests.cs  # 100k stream vs naive reference
benchmarks/
└── WordFinder.Benchmarks/
    ├── WordFinder.Benchmarks.csproj
    └── Program.cs
```

**Structure Decision**: Three projects (library, tests, benchmarks) keep production code separate, as the constitution requires. The library has no dependencies.

## Testing Strategy

Maps to the spec's acceptance scenarios and edge cases: horizontal and vertical matches; diagonal/reverse exclusion; boundaries without wrapping; overlaps (`aa` in `aaa`); single-code-unit words counted per direction; supplementary characters and individual or unpaired surrogate code units in both directions, including code-unit dimension limits; words longer than one dimension; duplicate entries; empty or unmatched streams; frequency, ordinal ties and the ten-result cap; case sensitivity and whitespace; matrix capture; repeated and concurrent independent searches; all 4,096 row/column dimension pairs against an independent reference, with horizontal and vertical matches, absent words, and occurrence-based ranking where applicable (SC-002); every invalid input case; and a 100,000-candidate comparison against a naive reference on the same workload used for measurement (SC-005). Automated workload tests verify the 64x64 dimensions, candidate count, reproducible fingerprint, and reference result; performance measurements remain observational and have no numeric pass/fail limits.

## Performance Validation

BenchmarkDotNet scenarios: 64x64 matrix with 100k mostly-absent words, 100k words with heavy duplication, and many matching words. At least one measured scenario must use exactly the same deterministic matrix and candidate stream as the naive reference comparison; record the seed or other reproducible generation details and a workload fingerprint. Use `[MemoryDiagnoser]` for allocations and elapsed time. Provide a `--peak-memory` benchmark-program mode that measures process peak working set in a fresh process for that same workload; distinguish it from total allocated bytes. Record workload, environment, timings, allocations, and peak working set in `research.md`; there are no numeric pass/fail limits. Benchmark ranking separately before claiming its cost is negligible, and compare the substring-index alternative before making a comparative speed claim.

## Complexity Tracking

Not applicable.
