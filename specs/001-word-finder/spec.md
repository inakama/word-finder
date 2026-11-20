# Feature Specification: WordFinder

**Feature Branch**: Not specified

**Created**: 2026-11-19

**Status**: Draft

**Input**: QuBeyond Developer Challenge: Word Finder

## Clarifications

### Session 2026-11-19

- Q: How should matrix cells and word lengths treat Unicode? → A: Count UTF-16 code units, including individual surrogate halves.
- Q: Should one WordFinder instance support simultaneous Find calls? → A: Concurrent Find calls on the same instance are supported.
- Q: Should the 100,000-candidate evaluation have mandatory performance limits? → A: Record time and peak memory without pass/fail limits.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Find words in a matrix (Priority: P1)

A caller supplies a rectangular character matrix and searches it for multiple words, receiving only words found in the permitted directions.

**Why this priority**: Finding words is the central purpose of the feature.

**Independent Test**: Search a small matrix containing horizontal and vertical matches and verify the returned words against the matrix.

**Acceptance Scenarios**:

1. **Given** a word occurring left to right in a row or top to bottom in a column, **When** it is searched for, **Then** it is returned if it ranks in the first ten by frequency, with equal-frequency words ordered by ascending ordinal (alphabetical) comparison.
2. **Given** a word occurring only diagonally or in a reverse direction, **When** it is searched for, **Then** it is not returned.
3. **Given** several search words, including absent words, **When** the matrix is searched, **Then** only the distinct words present are returned.
4. **Given** an empty word stream or no matching words, **When** the matrix is searched, **Then** the result is empty.
5. **Given** an initialized matrix, **When** the caller performs multiple searches, **Then** each search uses the same matrix and is unaffected by earlier searches.
6. **Given** an initialized matrix, **When** callers perform searches concurrently on the same instance, **Then** each search returns the same result it would return independently, without affecting the others.

### User Story 2 - Rank the most frequent words (Priority: P1)

A caller searching for many words receives up to ten distinct matches ordered by their frequency in the matrix.

**Why this priority**: Returning the most frequent matches is part of the challenge's core result.

**Independent Test**: Search a matrix containing repeated and overlapping matches for more than ten distinct words and verify counts, ordering, and the result limit.

**Acceptance Scenarios**:

1. **Given** words with different numbers of matrix occurrences, **When** they are searched for, **Then** the words with more occurrences appear first.
2. **Given** a word repeated in the word stream, **When** it is searched for, **Then** it appears at most once and its frequency is unchanged by those duplicates.
3. **Given** overlapping occurrences or occurrences in both permitted directions, **When** a word's frequency is determined, **Then** each qualifying start position and direction contributes one occurrence.
4. **Given** words with equal frequency, **When** they are returned, **Then** they are ordered by ascending ordinal comparison.
5. **Given** more than ten matching words, **When** the results are returned, **Then** only the first ten under the specified ranking are included.

### User Story 3 - Reject invalid input (Priority: P2)

A caller receives a clear failure for invalid matrix or search input rather than a misleading search result.

**Why this priority**: Predictable validation protects callers from malformed data.

**Independent Test**: Submit each invalid matrix and word-stream case and verify that the relevant operation rejects it.

**Acceptance Scenarios**:

1. **Given** a missing, empty, nonrectangular, or oversized matrix, or a matrix with a missing or empty row, **When** the matrix is initialized, **Then** initialization rejects it.
2. **Given** a missing word stream or a word stream containing a missing or empty word, **When** it is searched, **Then** the search rejects it.
3. **Given** a valid 1 by 1 or 64 by 64 matrix, **When** it is initialized and searched, **Then** the matrix is accepted and matches are returned according to the same rules.

### User Story 4 - Search a large word stream (Priority: P2)

A caller can search a large collection while relying on correct results and a documented evaluation of resource use.

**Why this priority**: Large collections are an explicit challenge constraint.

**Independent Test**: Search a stream of 100,000 candidates and compare the results with a trusted reference while recording elapsed time and peak memory.

**Acceptance Scenarios**:

1. **Given** a valid matrix and 100,000 candidate entries, **When** they are searched, **Then** the results match a trusted reference under the specified rules.
2. **Given** the same evaluation, **When** performance is assessed, **Then** elapsed time, peak memory, workload characteristics, and environment are recorded without numeric performance pass/fail limits.

### Edge Cases

- Words may match at the first or last position of a row or column; matches must not wrap across boundaries.
- Overlapping matches count separately: `aa` occurs twice left to right in `aaa`.
- A one-code-unit match contributes once horizontally and once vertically at each matching cell, because these are distinct permitted directions.
- A supplementary Unicode character occupies two cells; individual surrogate code units, including unpaired surrogates, remain valid input and are matched exactly.
- A word longer than both matrix dimensions cannot match; a word longer than one dimension can still match in the other.
- Matching is case-sensitive and exact; whitespace is treated as ordinary content, not trimmed.
- More than ten equally frequent matches are selected using the documented tie-breaking order.
- Missing or empty words invalidate the search even if earlier words match.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: WordFinder MUST capture a matrix at initialization and reuse that captured matrix for subsequent searches, unaffected by later changes to the caller's input. Concurrent searches on the same instance MUST be supported and produce independent results.
- **FR-002**: The matrix MUST contain 1 to 64 rows, each containing the same number of UTF-16 code units, from 1 to 64 inclusive; each code unit is one matrix cell. Missing matrices, missing or empty rows, and unequal row lengths MUST be rejected.
- **FR-003**: A search MUST accept a stream of words and find contiguous matches only left to right within a row and top to bottom within a column.
- **FR-004**: A search MUST exclude diagonal, reverse-direction, and boundary-crossing matches.
- **FR-005**: Matching and word identity MUST be exact and case-sensitive, without trimming or normalization. Word lengths and match positions MUST be measured in UTF-16 code units, including individual surrogate halves.
- **FR-006**: Each distinct input word MUST be considered once, regardless of how often it occurs in the word stream; absent words MUST be excluded.
- **FR-007**: Frequency MUST mean the number of matching start-position and direction pairs in the matrix, including overlaps and matches in both allowed directions. Duplicate stream entries MUST NOT increase frequency.
- **FR-008**: Results MUST contain at most ten distinct matched words, ordered by descending frequency; ties MUST be ordered by ascending ordinal comparison of the words, including when selecting the tenth result.
- **FR-009**: An empty word stream or a stream containing no matches MUST produce an empty result.
- **FR-010**: A missing word stream, or any missing or empty word within it, MUST be rejected; invalid words MUST NOT be silently skipped.
- **FR-011**: A rejected input MUST identify the invalid input to the caller; no partial result may be returned for an invalid search.
- **FR-012**: The solution MUST preserve the requested public WordFinder construction and Find search contract.
- **FR-013**: The solution MUST support large word streams while limiting unnecessary operations and memory use; performance decisions MUST be justified and meaningful performance claims validated by measurement.
- **FR-014**: The solution MUST be maintainable and independently testable, with automated coverage of each acceptance scenario and edge case.

### Key Entities

- **Matrix**: A rectangular, bounded collection of character rows used for all searches on a WordFinder instance.
- **Word stream**: A collection of candidate words supplied for one search; duplicate entries represent the same candidate.
- **Match occurrence**: A candidate word found at one start position in one permitted direction.
- **Search result**: An ordered collection of up to ten distinct matching words ranked by matrix occurrence frequency.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All acceptance scenarios and specified input-validation cases produce the documented outcome in automated checks.
- **SC-002**: Automated checks MUST verify all 4,096 matrix dimension pairs formed by 1 to 64 rows and 1 to 64 columns. For every pair, searches including horizontal and vertical matches and absent words MUST produce results identical to an independent reference algorithm, including occurrence-based ranking when applicable.
- **SC-003**: For streams with more than ten matching distinct words, exactly ten results are returned in the documented order; for streams with no matches, zero are returned.
- **SC-004**: Repeating any candidate in a word stream does not change the result or its ordering.
- **SC-005**: An evaluation of 100,000 candidate entries records elapsed time and peak memory, and confirms the same results as a trusted reference for the evaluated inputs; performance measurements have no numeric pass/fail limits.

## Assumptions

- Frequency measures matrix occurrences rather than stream repetitions: counting stream repetitions would conflict with the requirement to count duplicate input words only once.
- Start-position and direction pairs are counted separately, including for one-code-unit words; overlapping occurrences count independently.
- Equal-frequency words use ascending ordinal comparison as a deterministic default; comparison is case-sensitive and culture-independent.
- Missing inputs and malformed inputs are rejected rather than interpreted as empty; an explicitly empty word stream is valid.
- The matrix is captured at initialization so later changes to the caller's input do not change subsequent searches.
- The existing root SDK configuration is the source of truth for development compatibility. The search capability is a library, not a standalone user interface.