using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using WordFinder.Workloads;
using WordFinderApi = global::WordFinder.WordFinder;

if (args.Contains("--peak-memory", StringComparer.Ordinal))
{
	PeakMemoryMeasurement.Run();
	return;
}

BenchmarkRunner.Run<WordFinderBenchmarks>();

[MemoryDiagnoser]
[ShortRunJob]
public class WordFinderBenchmarks
{
	[Params(SearchScenario.MostlyAbsent, SearchScenario.DuplicateHeavy, SearchScenario.ManyMatches)]
	public SearchScenario Scenario { get; set; }

	private WordFinderApi finder = null!;
	private string[] candidates = [];

	[GlobalSetup]
	public void Setup()
	{
		var workload = DeterministicWorkload.Create();
		candidates = Scenario switch
		{
			SearchScenario.MostlyAbsent => workload.Candidates,
			SearchScenario.DuplicateHeavy => Enumerable.Range(0, DeterministicWorkload.CandidateCount)
				.Select(index => workload.Candidates[index % 20])
				.ToArray(),
			SearchScenario.ManyMatches => CreateMatchingCandidates(workload.Matrix),
			_ => throw new ArgumentOutOfRangeException(nameof(Scenario))
		};
		finder = new WordFinderApi(workload.Matrix);

		var fingerprint = DeterministicWorkload.ComputeFingerprint(workload.Matrix, candidates);
		Console.WriteLine($"Scenario={Scenario}; Matrix=64x64; Candidates={candidates.Length}; Seed={DeterministicWorkload.Seed}; Fingerprint={fingerprint}");
	}

	[Benchmark]
	public int FindTopTen() => finder.Find(candidates).Count();

	private static string[] CreateMatchingCandidates(string[] matrix)
	{
		var lines = matrix.Concat(Enumerable.Range(0, matrix[0].Length)
			.Select(columnIndex => new string(matrix.Select(row => row[columnIndex]).ToArray())))
			.ToArray();
		var matchingWords = lines.Select((line, index) => line.Substring(index % 8, 8)).ToArray();
		return Enumerable.Range(0, DeterministicWorkload.CandidateCount)
			.Select(index => matchingWords[index % matchingWords.Length])
			.ToArray();
	}
}

public enum SearchScenario
{
	MostlyAbsent,
	DuplicateHeavy,
	ManyMatches
}

internal static class PeakMemoryMeasurement
{
	public static void Run()
	{
		var workload = DeterministicWorkload.Create();
		var finder = new WordFinderApi(workload.Matrix);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var allocatedBefore = GC.GetTotalAllocatedBytes(true);
		var stopwatch = Stopwatch.StartNew();
		var results = finder.Find(workload.Candidates).ToArray();
		stopwatch.Stop();
		var allocatedBytes = GC.GetTotalAllocatedBytes(true) - allocatedBefore;
		using var process = Process.GetCurrentProcess();

		Console.WriteLine($"Matrix=64x64; Candidates={workload.Candidates.Length}; Seed={DeterministicWorkload.Seed}; Fingerprint={workload.Fingerprint}");
		Console.WriteLine($"ElapsedMilliseconds={stopwatch.Elapsed.TotalMilliseconds:F3}; AllocatedBytes={allocatedBytes}; PeakWorkingSetBytes={process.PeakWorkingSet64}; ResultCount={results.Length}");
	}
}
