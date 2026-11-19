# GitHub Copilot Instructions

## 1. Project Context

This repository contains a solution for the **Word Finder Developer Challenge**.

The goal is to implement an efficient word-search component that identifies words from an input stream within a character matrix.

The solution must prioritize:

- Correctness and adherence to the challenge requirements.
- Performance and efficient resource utilization.
- Readable, maintainable, and testable code.
- Clear technical decisions that can be explained and defended during a technical interview.

Do not introduce unnecessary complexity or infrastructure.

## 2. Technology Stack

Use the following technologies and conventions:

- **.NET 10 (LTS)**
- **C# 14**
- .NET Class Library for the core implementation.
- xUnit for automated tests.
- BenchmarkDotNet for performance benchmarks, if benchmarks are included.

Prefer built-in .NET functionality over external dependencies unless a dependency provides a clear benefit.

Do not introduce ASP.NET Core, Entity Framework, databases, messaging systems, Docker, or other infrastructure that is not required by the challenge.

## 3. Required Public API

The implementation must preserve the following public contract:

```csharp
public class WordFinder
{
    public WordFinder(IEnumerable<string> matrix)
    {
    }

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
    }
}
```

Do not rename public types or methods, change their signatures, or introduce additional mandatory parameters.

Internal implementation details may evolve as needed.

## 4. Functional Requirements

The implementation must support the following behavior:

- Accept a character matrix through the constructor.
- Search for words horizontally, from left to right.
- Search for words vertically, from top to bottom.
- Do not search diagonally or in reverse directions.
- Process words provided through the `wordstream`.
- Return at most the ten most frequently found words.
- Avoid counting the same input word multiple times when it appears repeatedly in the word stream.
- Return an empty collection when no words match.

The matrix is constrained by the challenge:

- Maximum dimensions: 64 rows by 64 columns.
- All rows must have the same length.

Do not assume additional functional requirements that are not explicitly stated in the challenge or agreed upon in the specification.

## 5. Specification-Driven Development

This project follows **GitHub Spec Kit** and a Specification-Driven Development workflow.

Before implementing functionality:

1. Read the relevant specification in `specs/`.
2. Read the corresponding implementation plan.
3. Review the task list.
4. Identify unresolved questions or conflicting requirements.
5. Implement only behavior that is supported by the specification or explicitly agreed upon.

The specification is the source of truth for functional behavior.

If a requirement is ambiguous, do not silently make assumptions. Document the ambiguity and request clarification or propose an explicit decision before implementing it.

Keep the following documents aligned:

- `.specify/memory/constitution.md`: project-wide engineering principles.
- `specs/001-word-finder/spec.md`: functional requirements and acceptance criteria.
- `specs/001-word-finder/plan.md`: technical design and implementation decisions.
- `specs/001-word-finder/tasks.md`: implementation tasks.

When implementation decisions change, update the relevant documentation.

## 6. Architecture and Design Guidelines

Keep the solution simple and focused.

- Separate the core implementation from tests and benchmarks.
- Keep the public API small.
- Encapsulate implementation details behind the `WordFinder` class.
- Prefer small, cohesive methods with clear responsibilities.
- Avoid unnecessary abstractions, interfaces, factories, or design patterns.
- Avoid premature optimization.

Evaluate algorithmic alternatives based on the matrix constraints and expected workload.

Consider time complexity, space complexity, memory allocations, and repeated work.

A Trie or another specialized data structure may be considered, but must not be introduced without a clear justification.

Document relevant trade-offs in `plan.md`, including why the selected approach is appropriate for this challenge.

## 7. C# Coding Standards

Follow modern C# and .NET conventions.

- Use meaningful names for types, methods, and variables.
- Prefer explicit and clear code over clever or overly compact implementations.
- Use appropriate access modifiers.
- Keep implementation details private unless they are part of the required public contract.
- Use nullable reference types.
- Avoid unnecessary allocations and repeated enumerations of input sequences.
- Avoid mutable shared state unless it is required and properly controlled.
- Use LINQ when it improves readability without introducing unnecessary performance costs.
- Handle invalid inputs consistently with documented behavior.

Do not add comments that merely repeat what the code already expresses. Add comments when they explain non-obvious algorithms, constraints, or trade-offs.

## 8. Testing Requirements

Use xUnit for automated tests.

Tests must cover, at minimum:

- Horizontal word matches.
- Vertical word matches.
- Words that do not exist in the matrix.
- Matches at matrix boundaries.
- Multiple occurrences of the same word.
- Repeated words in the input stream.
- Empty word streams.
- No matching words.
- Results limited to the ten most frequent words.
- Frequency ordering.
- Equal-frequency behavior, according to the documented tie-breaking rule.
- Input validation, according to the documented contract.

Include edge cases and regression tests for any discovered defects.

Tests should validate observable behavior, not private implementation details.

Do not modify or weaken tests simply to make an incorrect implementation pass.

## 9. Performance Considerations

Performance is an explicit concern of the challenge.

When designing or modifying the implementation:

- Avoid scanning the entire matrix independently for every input word if a more efficient approach is justified.
- Avoid unnecessary string allocations.
- Avoid repeated enumeration of `IEnumerable<T>` inputs.
- Consider preprocessing the matrix when it reduces repeated work.
- Consider the trade-off between preprocessing cost, lookup time, and memory usage.
- Use benchmarks when performance claims need to be validated.

Do not claim that an implementation is faster based only on intuition. Support meaningful performance claims with complexity analysis or benchmark results.

Correctness must not be sacrificed for performance.

## 10. Error Handling and Input Validation

Follow the behavior defined in the specification for null, empty, and invalid inputs.

If the specification does not define a behavior:

- Identify the ambiguity.
- Propose a consistent behavior.
- Document the decision before relying on it.

Do not silently add restrictive validation that changes the expected public contract.

## 11. Implementation Workflow

For each implementation task:

1. Understand the relevant requirement.
2. Check the acceptance criteria.
3. Review the existing code before modifying it.
4. Implement the smallest coherent change.
5. Add or update automated tests.
6. Run the relevant tests.
7. Review the implementation for correctness and unnecessary complexity.
8. Update documentation when technical decisions or behavior change.

Avoid implementing unrelated features or refactoring unrelated code.

## 12. AI-Assisted Development

GitHub Copilot is an engineering assistant, not the decision-maker.

When generating code or proposing changes:

- Explain non-obvious design decisions.
- Identify assumptions explicitly.
- Prefer solutions that are easy to understand and defend.
- Do not invent requirements.
- Do not introduce dependencies without justification.
- Do not claim tests or benchmarks were executed unless they actually were.
- Highlight potential edge cases and limitations.

The final implementation must be understandable by the developer and defensible in a technical interview.

## 13. Definition of Done

A task is complete when:

- Its acceptance criteria are satisfied.
- The implementation follows the agreed technical design.
- Relevant automated tests pass.
- No unnecessary dependencies or infrastructure have been introduced.
- The code is readable and maintainable.
- Relevant documentation is up to date.
- Performance considerations have been addressed where applicable.