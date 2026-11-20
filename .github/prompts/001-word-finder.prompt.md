---
name: 001-word-finder
description: Creates the WordFinder specification based on the QuBeyond Developer Challenge, validating global.json as a mandatory prerequisite.
agent: speckit.specify
---

# Create the WordFinder Specification

Create the specification for the `001-word-finder` initiative, based on the requirements of the QuBeyond Developer Challenge: Word Finder.

## Main Instructions

Strictly follow:

- `.specify/memory/constitution.md`
- Existing conventions and architectural decisions in the repository.
- The functional and technical requirements defined in this prompt.

The specification must prioritize code quality, simplicity, algorithmic efficiency, maintainability, and testability.

## Mandatory Prerequisite (Blocking)

Before generating any specification files:

1. Verify that the `global.json` file exists in the repository root.
2. Use `global.json` as the single source of truth to determine the .NET version that will be used in this and future .NET project initiatives.
3. Verify that the configured version is compatible with the development environment.

If `global.json` does not exist:

- Stop the entire process.
- Do not create `spec.md`, `plan.md`, or `tasks.md`.
- Display exactly the following error message to the user:

ERROR: global.json was not found in the repository root. You must create this file first to determine the .NET version that will be used by .NET projects.

## Objective

Develop a solution called `WordFinder` that allows searching for words within a character matrix and returns the 10 most frequently found words.

The solution must be designed to process large collections of words, prioritizing algorithm performance and efficient system resource utilization.

## Initiative Type

Feature

## Functional Requirements

### 1. WordFinder Class

Implement a public class named `WordFinder` with the following interface:

```csharp
public class WordFinder
{
    public WordFinder(IEnumerable<string> matrix)
    {
        // ...
    }

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
        // ...
    }
}
```

### 2. Matrix Initialization

The constructor must receive a collection of strings representing a character matrix.

Constraints:

- The matrix cannot exceed 64 rows by 64 columns.
- All rows must contain the same number of characters.
- The matrix must support horizontal and vertical searches.
- The matrix must be initialized only once in the constructor and reused for subsequent searches.

### 3. Word Search

The `Find` method must receive a collection of words (`wordstream`) and determine which ones are present in the matrix.

Searches must consider exclusively the following directions:

- Horizontal: left to right.
- Vertical: top to bottom.

Diagonal searches and reverse-direction searches must not be considered.

### 4. Results

The `Find` method must comply with the following rules:

- Return at most the 10 most frequent words found in the matrix.
- Words that are not present in the matrix must be excluded from the results.
- If a word appears more than once in `wordstream`, it must be counted only once.
- If no words are found, an empty collection must be returned.
- Results must follow the frequency criteria established by the challenge.

**Important:** The specification must analyze and explicitly define what frequency means in the context of this exercise, considering that duplicate words in `wordstream` must be counted only once. Do not assume an interpretation without documenting the decision.

## Non-Functional Requirements

### 1. Performance

The implementation must prioritize:

- Algorithmic efficiency.
- Reduction of unnecessary operations.
- Efficient memory usage.
- Avoiding data structures with unjustified resource consumption.
- The ability to process large collections of words.

Consider that the matrix has a maximum size of 64 × 64, while `wordstream` may contain a considerable number of elements.

### 2. Code Quality

The solution must:

- Follow SOLID principles where appropriate.
- Maintain clearly defined responsibilities.
- Use descriptive naming conventions.
- Avoid unnecessary complexity.
- Be easy to test and maintain.
- Leverage C# language features and the .NET version configured in `global.json`.

### 3. Validation and Error Handling

The specification must address invalid input scenarios, including:

- Null or empty matrix.
- Null or empty rows.
- Rows with different lengths.
- Dimensions exceeding 64 × 64.
- Null or empty `wordstream` collection.
- Null or empty words.

Define the expected behavior for each scenario and document the corresponding decisions.

## Testing Requirements

The solution must include unit tests covering, at a minimum:

1. Searching for a horizontal word.
2. Searching for a vertical word.
3. Searching for multiple words.
4. Words that do not exist in the matrix.
5. Duplicate words in `wordstream`.
6. Returning a maximum of 10 results.
7. Returning an empty collection when there are no matches.
8. Matrix with minimum dimensions.
9. Matrix with maximum