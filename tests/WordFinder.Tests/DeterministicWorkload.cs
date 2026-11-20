using System.Security.Cryptography;
using System.Text;

namespace WordFinder.Workloads;

public static class DeterministicWorkload
{
    public const int Seed = 20261119;
    public const int CandidateCount = 100_000;

    public static (string[] Matrix, string[] Candidates, string Fingerprint) Create()
    {
        var random = new Random(Seed);
        var matrix = Enumerable.Range(0, 64)
            .Select(_ => new string(Enumerable.Range(0, 64).Select(_ => (char)('a' + random.Next(26))).ToArray()))
            .ToArray();
        var columns = Enumerable.Range(0, 64)
            .Select(columnIndex => new string(matrix.Select(row => row[columnIndex]).ToArray()))
            .ToArray();
        var candidates = new string[CandidateCount];

        for (var index = 0; index < candidates.Length; index++)
        {
            if (index % 20 == 0)
            {
                var line = random.Next(2) == 0 ? matrix[random.Next(matrix.Length)] : columns[random.Next(columns.Length)];
                var wordLength = random.Next(2, 13);
                var startIndex = random.Next(line.Length - wordLength + 1);
                candidates[index] = line.Substring(startIndex, wordLength);
            }
            else if (index % 20 == 1)
            {
                candidates[index] = candidates[index - 1];
            }
            else
            {
                candidates[index] = new string(Enumerable.Range(0, 8).Select(_ => (char)('a' + random.Next(26))).ToArray());
            }
        }

        var fingerprint = ComputeFingerprint(matrix, candidates);
        return (matrix, candidates, fingerprint);
    }

    public static string ComputeFingerprint(string[] matrix, string[] candidates)
    {
        var fingerprintInput = string.Join('\n', matrix) + "\n--\n" + string.Join('\n', candidates);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintInput)));
    }
}