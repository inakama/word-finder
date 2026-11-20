# Tasks: WordFinder

**Input**: Design documents from `specs/001-word-finder/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/public-api.md`, `quickstart.md`

**Tests**: Required by the specification; write behavior-changing story tests before implementation and verify that they fail for the intended missing behavior. The US4 reference comparison validates behavior already delivered by US1-US3 and may pass immediately.

**Format**: `- [ ] T### [P?] [US#?] Description with file path`; `[P]` identifies tasks that can run concurrently once their stated prerequisites are complete and that do not edit the same file.

## Phase 1: Setup

**Purpose**: Create the library, test project, and solution using the SDK pinned by `global.json`.

- [X] T001 Create the .NET 10 class library with nullable reference types and no production dependencies in `src/WordFinder/WordFinder.csproj`.
- [X] T002 Create the .NET 10 xUnit project and reference the library in `tests/WordFinder.Tests/WordFinder.Tests.csproj`.
- [X] T003 Create `WordFinder.slnx` and add `src/WordFinder/WordFinder.csproj` and `tests/WordFinder.Tests/WordFinder.Tests.csproj`.

## Phase 2: Foundational

**Purpose**: Establish the exact public API and ensure the projects build before story work begins.

- [X] T004 Create the public `WordFinder` class with the exact constructor and `Find` signatures from `specs/001-word-finder/contracts/public-api.md` in `src/WordFinder/WordFinder.cs`; leave behavior explicitly unimplemented until story tasks.
- [X] T005 Build `WordFinder.slnx` and resolve project configuration or API signature issues before starting story tests.

**Checkpoint**: The solution builds and the public API is available for tests; user stories remain unimplemented.

## Phase 3: User Story 1 - Find words in a matrix (Priority: P1)

**Goal**: Search a captured matrix horizontally and vertically for distinct words.

**Independent Test**: Search a small matrix with horizontal, vertical, absent, diagonal-only and reverse-only candidates; verify UTF-16 code-unit matches and compare concurrent calls with independent results after mutating the caller's row collection.

### Tests

- [X] T006 [US1] Add observable search tests in `tests/WordFinder.Tests/FindTests.cs` for horizontal and vertical matches, multiple and absent words, empty streams, no matches, and exclusion of diagonal, reverse, and boundary-wrapping matches.
- [X] T007 [US1] Extend `tests/WordFinder.Tests/FindTests.cs` with first/last boundary matches, case-sensitive, untrimmed and non-normalizing matching, non-square matrices and words too long for one direction but matching the other, matrix capture, repeated calls and one-time enumeration; test supplementary characters and individual or unpaired surrogate code units in both directions, and coordinated concurrent calls with distinct streams against their independent results.

### Implementation

- [X] T008 [US1] Capture valid matrix rows once and precompute columns by UTF-16 code-unit index in `src/WordFinder/WordFinder.cs`, retaining unpaired surrogates and keeping state private and immutable for concurrent searches.
- [X] T009 [US1] Implement ordinal, distinct-word searches across individual rows and columns in `src/WordFinder/WordFinder.cs`, with code-unit length prefilters, per-call mutable state and empty results; run the US1 tests in `tests/WordFinder.Tests/FindTests.cs`.

**Checkpoint**: US1 tests pass independently; frequency ranking and full invalid-input handling are added in later stories.

## Phase 4: User Story 2 - Rank the most frequent words (Priority: P1)

**Goal**: Return at most ten distinct matches ordered by matrix occurrence frequency and ordinal ties.

**Independent Test**: Search a matrix with overlaps, both directions, duplicate candidates, and more than ten matching words; check the complete expected order and cutoff.

### Tests

- [X] T010 [US2] Add ranking tests in `tests/WordFinder.Tests/FindTests.cs` for unequal frequencies, overlapping occurrences, occurrences in both directions, and one-code-unit matches counted in both directions using observable result ordering.
- [X] T011 [US2] Add tests in `tests/WordFinder.Tests/FindTests.cs` for duplicate stream entries, ordinal case-sensitive ties, more than ten equal-frequency matches at the cutoff, and the ten-result cap.

### Implementation

- [X] T012 [US2] Count each distinct word's overlapping ordinal occurrences in rows and columns using `ReadOnlySpan<char>.IndexOf` in `src/WordFinder/WordFinder.cs`, advancing one UTF-16 code unit after each hit and counting single-code-unit hits once per direction.
- [X] T013 [US2] Rank positive-count matches by descending count and ascending ordinal word, return at most ten distinct words in `src/WordFinder/WordFinder.cs`, and run the US1 and US2 tests in `tests/WordFinder.Tests/FindTests.cs`.

**Checkpoint**: The complete search and ranking behavior is testable through the public API.

## Phase 5: User Story 3 - Reject invalid input (Priority: P2)

**Goal**: Reject malformed matrices and word streams with the exceptions and argument identification specified by the public contract.

**Independent Test**: Exercise every invalid constructor and search input, including a late invalid word, and verify valid 1x1 and 64x64 matrices.

### Tests

- [X] T014 [US3] Add constructor contract tests in `tests/WordFinder.Tests/ValidationTests.cs` for null and empty matrices, null and empty rows, unequal row lengths, 65 rows, 65 columns, valid 1x1 and 64x64 matrices, UTF-16 code-unit dimension limits including supplementary characters and valid unpaired surrogates, exception types, and parameter names.
- [X] T015 [US3] Add `Find` contract tests in `tests/WordFinder.Tests/ValidationTests.cs` for null streams, null and empty words, an invalid word after a matching word, empty valid streams, exception types, and parameter names; assert no partial result is returned.

### Implementation

- [X] T016 [US3] Validate the matrix during its single enumeration in `src/WordFinder/WordFinder.cs`, measuring row lengths in UTF-16 code units without rejecting unpaired surrogates; reject nulls with `ArgumentNullException` and invalid dimensions or row shapes with `ArgumentException`, stopping when the 65th row is encountered.
- [X] T017 [US3] Validate the entire stream exactly once into an ordinal distinct-word set before searching in `src/WordFinder/WordFinder.cs`, rejecting nulls with `ArgumentNullException` and empty words with `ArgumentException`; run `tests/WordFinder.Tests/ValidationTests.cs` and the search tests.

**Checkpoint**: The public constructor and search contract, including invalid input, pass automated tests.

## Phase 6: User Story 4 - Search a large word stream (Priority: P2)

**Goal**: Verify correctness for all 4,096 supported matrix dimension pairs and for a 100,000-candidate workload, then document reproducible resource measurements.

**Independent Test**: For every row count and column count from 1 through 64, compare the complete ordered results against an independent reference for deterministic matrices and candidate words covering horizontal and vertical matches, absent words, and occurrence-based ranking. Also compare the 100,000-candidate workload against the reference and record elapsed time and peak memory with workload and environment details.

### Tests and measurement setup

- [X] T018 [P] [US4] Implement a reusable deterministic 64x64 matrix and 100,000-candidate workload generator and fingerprint in `tests/WordFinder.Tests/DeterministicWorkload.cs`, and automated tests in `tests/WordFinder.Tests/ReferenceComparisonTests.cs` that verify workload dimensions/count, fingerprint reproducibility, and an independent naive reference comparison; cover duplicates, absent words and matches, compare the complete ranked result, and record the seed or generation details and fingerprint.
- [X] T019 [US4] Add an automated reference-comparison test in `tests/WordFinder.Tests/ReferenceComparisonTests.cs` that generates and tests every matrix dimension pair from 1x1 through 64x64 (4,096 combinations), exercising horizontal and vertical matches and absent words, and asserts exact equality with the independent reference result, including occurrence-based counts and ranking where applicable.
- [X] T020 [P] [US4] Create the .NET 10 BenchmarkDotNet project with `MemoryDiagnoser` and a reference to the library in `benchmarks/WordFinder.Benchmarks/WordFinder.Benchmarks.csproj`.

### Implementation and evaluation

- [X] T021 [US4] Add `benchmarks/WordFinder.Benchmarks/WordFinder.Benchmarks.csproj` to `WordFinder.slnx` after the benchmark project exists.
- [X] T022 [US4] Link `tests/WordFinder.Tests/DeterministicWorkload.cs` into `benchmarks/WordFinder.Benchmarks/WordFinder.Benchmarks.csproj`, then implement 64x64 and 100,000-candidate BenchmarkDotNet scenarios in `benchmarks/WordFinder.Benchmarks/Program.cs` for mostly absent, duplicate-heavy, and many matching words; one scenario must use the exact T018 matrix and ordered stream, report its fingerprint, elapsed time and allocations so automated tests can verify the workload identity.
- [X] T023 [US4] Implement the `--peak-memory` mode in `benchmarks/WordFinder.Benchmarks/Program.cs` to measure process peak working set in a fresh process for the exact T018 workload and report its fingerprint; distinguish peak working set from allocated bytes.
- [X] T024 [US4] Run the exhaustive dimension comparison from T019, the 100,000-candidate comparison from T018, and both Release commands in `specs/001-word-finder/quickstart.md`; verify matching workload fingerprints and record seed or generation details, workload characteristics, environment, elapsed time, allocations and peak working set in `specs/001-word-finder/research.md`, without numeric pass/fail limits or unsupported speed claims.

**Checkpoint**: The large-stream result matches the reference and the required measurement record exists.

## Phase 7: Polish & Cross-Cutting Concerns

- [X] T025 [P] Review `src/WordFinder/WordFinder.cs` against `specs/001-word-finder/contracts/public-api.md` for exact signatures, immutable instance state, single enumeration, and no unnecessary production dependencies.
- [X] T026 [P] Verify the manual example and build, test, and benchmark instructions in `specs/001-word-finder/quickstart.md`, updating them only if implementation or measured behavior requires it.
- [X] T027 Run the complete build and xUnit suite for `WordFinder.slnx`, and ensure the acceptance and edge cases in `specs/001-word-finder/spec.md`, including the 4,096 dimension comparisons, are covered without weakening tests.

## Dependencies & Execution Order

### Phase dependencies

- Setup -> Foundational -> US1 -> US2 -> US3 -> US4 -> Polish.
- US1 establishes the public search behavior. US2 extends that behavior with ranking. US3 adds the complete validation contract to the same class. US4 evaluates the completed search and validation behavior.
- US1 is independently demonstrable as an interim MVP, but the challenge's required top-ten ranking and validation are not complete until US2 and US3 respectively.
- Story tests can be prepared after their infrastructure exists, but changes to `src/WordFinder/WordFinder.cs` and `tests/WordFinder.Tests/FindTests.cs` must be coordinated and applied in phase order.

### Within each story

- For US1-US3, write tests first and observe expected failures before implementing missing behavior. US4's reference comparison validates the existing implementation and need not fail first.
- Run the story's focused tests after implementation; preserve all previously passing story tests.
- After US3, T018 (the 100,000-candidate reference workload) and T020 (benchmark project setup) can proceed independently. T019 follows T018 because both edit `ReferenceComparisonTests.cs`. T021 depends on T020; T022 depends on T018 and T020; T023 depends on T022; T024 runs after the comparison tests and measurement modes are complete.

## Parallel Examples

- **US1**: After T005, one contributor can write T006/T007 while another reviews the contract and plans T008/T009; implementation starts only after the new tests fail.
- **US2**: After US1, one contributor can write T010/T011 while another plans T012/T013; implementation starts only after the new tests fail.
- **US3**: After US2, one contributor can write T014/T015 while another plans T016/T017; implementation starts only after the new tests fail.
- **US4**: After US3, T018 and T020 can run concurrently because they edit different files; complete T019 after T018, then proceed with the benchmark tasks as their dependencies allow.
- T025 and T026 can run concurrently after US4; neither changes the other's file.

## Implementation Strategy

1. Complete setup and the API foundation, then deliver and independently test US1 as the search MVP.
2. Add US2 ranking and US3 validation in order, rerunning earlier tests after each increment to complete the required challenge contract.
3. Add US4's reference comparison and reproducible performance measurements, recording results without assuming the scan is faster than an alternative.
4. Finish with the cross-cutting review and full solution checks.