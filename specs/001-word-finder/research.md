# Research: WordFinder

No `NEEDS CLARIFICATION` items remain. The decisions below resolve the open design choices.

## 1. Search algorithm

- **Decision**: Per distinct word, count overlapping ordinal occurrences in each row and precomputed column using span `IndexOf`.
- **Rationale**: Each distinct word is searched in at most 128 lines, each at most 64 UTF-16 code units long; repeated searches for overlaps also contribute work. Deduplication avoids repeating searches for duplicate candidates. The approach is simple, while its actual runtime and allocations remain subject to measurement.
- **Alternatives considered**:
  - *Substring index* (dictionary of every row/column substring with counts, queried with `GetAlternateLookup<ReadOnlySpan<char>>`): O(1) lookup per word but up to ~262k entries and string allocations at construction. Hypothesis to evaluate, not an established advantage: it may help when one matrix serves many large searches, pending a direct benchmark against the selected scan approach.
  - *Trie of words + matrix walk*: requires building the trie from the whole stream and adds code and allocations; benefit is unproven at this scale.
  - *Aho-Corasick over rows/columns*: asymptotically strongest for huge streams, but too complex for this challenge's bounds.
  - *Naive nested character loops*: used only as the test reference implementation.

## 2. Matrix capture

- **Decision**: Store `string[] rows` and `string[] columns`.
- **Rationale**: Satisfies FR-001 (caller changes do not affect searches) and gives both directions contiguous spans. Cost is 2 × 4,096 chars.
- **Alternatives**: `char[,]` (no span search without copying); keeping the caller's enumerable (violates FR-001 and re-enumeration concerns).

## 3. Input validation and errors

- **Decision**: `ArgumentNullException` for null matrix/rows/stream/words; `ArgumentException` for empty rows/words, empty matrix, unequal row lengths, or more than 64 rows/columns. Validate the entire stream before returning anything.
- **Rationale**: Idiomatic .NET, identifies the invalid argument (FR-011), no partial results.
- **Note**: The matrix is enumerated once. Check the 64-row limit while enumerating so oversized inputs stop early.

## 4. Ranking

- **Decision**: Sort matches by count descending, then `string.CompareOrdinal`; take 10.
- **Rationale**: Deterministic and culture-independent (FR-008). The match set is bounded by distinct words. Whether sorting is significant relative to scanning is an unverified hypothesis; measure it separately before drawing a conclusion. Heap-based top-10 is a later option if benchmarks justify it.

## 5. Word length pre-filter

- **Decision**: Skip words longer than `max(rows, columns)` before scanning; skip a direction when the line length is shorter than the word.
- **Rationale**: Avoids useless work for the common case of random or over-long candidates and keeps boundary/wrap behavior trivially correct (a line is searched in isolation, so no wrapping).

## 6. Performance evaluation (SC-005)

- **Decision**: Use the same deterministic 64x64 matrix and 100,000-candidate workload for an independent naive-reference comparison and the measured search. Record BenchmarkDotNet elapsed time and allocations, and measure process peak working set separately in an isolated process. Record workload characteristics and environment; no numeric performance pass/fail limits apply.
- **Rationale**: Allocated bytes are not peak memory. A shared workload connects the correctness comparison to the measured search. Results and environment are to be added to this file after implementation; comparative claims require an appropriate benchmark.

## 7. Dependencies

- **Decision**: Library has none. xUnit (tests) and BenchmarkDotNet (benchmarks) only; versions resolved when the projects are created.

## 8. Text units and concurrency

- **Decision**: Treat each UTF-16 code unit as one cell and use ordinal code-unit matching, including individual and unpaired surrogates. Capture matrix state once and keep each search's mutable state local to its call.
- **Rationale**: These are the clarified contract rules. They allow independent concurrent searches without shared mutable search state.
- **Alternatives considered**: Unicode scalar values or grapheme clusters would change dimensions and matching semantics; shared mutable search state would require coordination between calls.

## 9. Implementation measurements

The reference-comparison tests passed for all 4,096 supported matrix dimensions and for the 100,000-candidate workload. The workload and its ordered candidate stream are generated with `Random(20261119)`: a 64x64 matrix of lowercase ASCII letters; 5% of candidates are matrix substrings of length 2-12, 5% repeat the preceding candidate, and 90% are random eight-letter candidates. Its SHA-256 fingerprint is `EFB46AB1D8D7FBE318023B7AEF967C5DE5C0D025D9DE04F5002693B68FFDD055`.

Environment: Windows 11 (10.0.26300.9974), .NET SDK 10.0.404, .NET runtime 10.0.15, BenchmarkDotNet 0.15.8, Intel Core i5-8250U (1.60 GHz, maximum 1.80 GHz), x64 RyuJIT, workstation concurrent GC.

The `--peak-memory` mode ran the deterministic 100,000-candidate workload in a separate process. `Find` took 276.480 ms and allocated 6,453,104 bytes; the process peak working set was 46,264,320 bytes. Peak working set includes process startup and workload construction, while allocated bytes and elapsed time cover the `Find` call.

BenchmarkDotNet used a ShortRun (one launch, three warmups, three measured iterations). The deterministic mostly-absent scenario used the exact fingerprint above; the other scenario fingerprints identify their respective ordered streams.

| Scenario | Candidate entries | Fingerprint | Mean | Allocated |
|----------|-------------------|-------------|------|-----------|
| Mostly absent | 100,000 | `EFB46AB1D8D7FBE318023B7AEF967C5DE5C0D025D9DE04F5002693B68FFDD055` | 409.233 ms | 6,301.64 KiB |
| Duplicate-heavy | 100,000, repeating the first 20 base entries | `880E03CC9D443869EFA81D34D3E01E2C6DC1CC470BF62A761633CE7088F8871D` | 3.737 ms | 2.26 KiB |
| Many matches | 100,000, cycling through row/column substrings | `9C3156E48D4EB53819862FFC3AB2C78DD7E2CFDB84049A84271F4E0437C0BFF2` | 3.287 ms | 15.97 KiB |

These are observational measurements, not pass/fail limits. The short run had substantial variance, particularly for the first two scenarios; no comparative performance conclusion is drawn.
