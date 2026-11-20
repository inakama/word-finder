# Contract: WordFinder public API

```csharp
public class WordFinder
{
    public WordFinder(IEnumerable<string> matrix);
    public IEnumerable<string> Find(IEnumerable<string> wordstream);
}
```

Names and signatures MUST NOT change (FR-012).

## Constructor

- Input: rows of a rectangular matrix, 1-64 rows × 1-64 columns, with one UTF-16 code unit per cell. Supplementary characters occupy two cells; unpaired surrogates are valid.
- Captures the data; later changes to the caller's collection have no effect.
- Throws `ArgumentNullException` for a null matrix or null row.
- Throws `ArgumentException` for an empty matrix, empty row, unequal row lengths, or dimensions above 64.

## Find

- Input: word stream, enumerated once. Empty is valid.
- Returns at most 10 distinct matched words, ordered by occurrence count descending, then ordinal ascending. Empty when nothing matches.
- Matching: exact, case-sensitive, ordinal UTF-16 code units, including individual or unpaired surrogates; left-to-right within rows and top-to-bottom within columns; overlaps counted; no diagonal, reverse, or wrapping matches. Word lengths and match positions use code units.
- Throws `ArgumentNullException` for a null stream or null word; `ArgumentException` for an empty word. No partial result on failure.
- Safe for repeated and concurrent calls on the same instance, with independent results. Callers must ensure any enumerable shared between concurrent calls supports concurrent enumeration.
