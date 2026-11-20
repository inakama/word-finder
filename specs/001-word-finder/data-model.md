# Data Model: WordFinder

## Matrix (internal state of `WordFinder`)

| Field | Type | Notes |
|-------|------|-------|
| rows | `string[]` | 1-64 entries; each non-empty; all same length (1-64 UTF-16 code units). Collection copied at construction. |
| columns | `string[]` | One string per UTF-16 code-unit column index, built from `rows` at construction. |

**Validation** (constructor, FR-002): matrix not null; at least 1 and at most 64 rows; no null or empty row; equal row lengths; length ≤ 64 UTF-16 code units. Unpaired surrogates are valid code units.

**Lifecycle**: immutable after construction. No transitions. Concurrent searches read the same captured state and keep mutable search state per call.

## Word stream (input to `Find`)

- Enumerated exactly once.
- Each word: not null, not empty (FR-010). Whitespace, case, and individual UTF-16 code units, including unpaired surrogates, are significant (FR-005).
- Reduced to a `HashSet<string>` (ordinal) of distinct words (FR-006). An empty stream is valid.

## Match occurrence (conceptual)

One `(start cell, direction)` pair where the word matches; direction ∈ {horizontal, vertical}. Positions and word lengths use UTF-16 code units. Overlaps count separately; a one-code-unit word contributes two occurrences per matching cell.

## Match result entry (private)

| Field | Type | Notes |
|-------|------|-------|
| Word | `string` | Distinct matched word |
| Count | `int` | Number of occurrences, > 0 |

**Ordering**: Count descending, then `string.CompareOrdinal(Word)` ascending. Output is the first 10 as `IEnumerable<string>`.
