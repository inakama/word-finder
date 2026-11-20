# Quickstart: Validating WordFinder

**Prerequisites**: .NET SDK 10.0.404 (see `global.json`).

## Build and test

```powershell
dotnet build
dotnet test
```

Expected: all tests pass, covering every acceptance scenario and edge case in [spec.md](./spec.md), including UTF-16 surrogate handling, concurrent calls, and the 100,000-candidate reference comparison.

## Manual check

Using the contract in [contracts/public-api.md](./contracts/public-api.md), create a `WordFinder` from `["abcdc", "fgwio", "chill", "pqnsd", "uvdxy"]` and call `Find(["cold", "wind", "snow", "chill"])`. Expected: `chill` (horizontal), `cold` (vertical) and `wind` (vertical) each occur once, so the result is `chill`, `cold`, `wind` (ordinal tie-break); `snow` is absent.

## Benchmarks

```powershell
dotnet run -c Release --project benchmarks/WordFinder.Benchmarks
dotnet run -c Release --project benchmarks/WordFinder.Benchmarks -- --peak-memory
```

Expected: elapsed time and allocations per scenario, plus process peak working set from the separate `--peak-memory` run. The reference comparison and at least one measured scenario use the same reproducible 100,000-candidate workload (see [plan.md](./plan.md)). Record workload details, environment, and results in [research.md](./research.md); no numeric pass/fail limits apply.
