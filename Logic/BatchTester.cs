using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// A bunch of string chikanery I dont care about

public static class BatchTester
{
    public const string OutDir = "res://batch";

    public class RunResult
    {
        public int Seed;
        public bool Solved;
        public bool Aborted;
        public int PathMoves = -1; // moves in the found path, -1 if none
        public int Iterations, MaxOpen, OpenAtEnd, MaxStored;
        public double TimeMs;
        public string Error = "";
    }

    public static string Run(string method, string heuristic, int count, int startSeed,
                             int width, int height, float fireChance,
                             Action<int, int> onProgress = null)
    {
        var results = new List<RunResult>(count);

        var rng = new RandomNumberGenerator { Seed = (ulong)startSeed };
        
        var batchTimer = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
        {
            int seed = (int)rng.Randi();
            var result = RunOne(method, heuristic, seed, width, height, fireChance);
            results.Add(result);

            GD.Print($"[{i + 1}/{count}] seed {seed}: " +
                    $"{(result.Aborted ? "aborted" : result.Solved ? "solved" : "no solution")}, " +
                    $"{result.Iterations} iters, {result.TimeMs:0.#} ms");

            onProgress?.Invoke(i + 1, count);
        }
        batchTimer.Stop();

        // file
        string inv(float f) => f.ToString("0.###", CultureInfo.InvariantCulture);
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string baseName = $"{method}_{heuristic}_{width}x{height}_n{count}_seed{startSeed}_{stamp}";

        DirAccess.MakeDirRecursiveAbsolute(OutDir);
        string ignorePath = $"{OutDir}/.gdignore"; //so godot wont think our csv is translation table
        if (!Godot.FileAccess.FileExists(ignorePath))
            Write(ignorePath, "");
        string csvPath = $"{OutDir}/{baseName}.csv";
        string sumPath = $"{OutDir}/{baseName}_summary.txt";

        Write(csvPath, BuildCsv(results));
        Write(sumPath, BuildSummary(results, method, heuristic, count, startSeed,
                                    width, height, fireChance, batchTimer.Elapsed.TotalMilliseconds));

        string global = ProjectSettings.GlobalizePath(csvPath);
        GD.Print("Batch written to ", global);
        return global;
    }

    public static Task<string> RunAsync(string method, string heuristic, int count, int startSeed,
                                    int width, int height, float fireChance,
                                    Action<int, int> onProgress = null)
    => Task.Run(() => Run(method, heuristic, count, startSeed, width, height, fireChance, onProgress));

    // single run
    static RunResult RunOne(string method, string heuristic, int seed, int w, int h, float chance)
    {
        var r = new RunResult { Seed = seed };
        
        State board = BoardGenerator.Generate(seed, w, h, chance);

        var solver = new Solver();
        solver.SetHeuristic(heuristic);
        var tree = solver.GetTree(method, board);

        // tree.timer is never stopped, so read it right away
        r.TimeMs = tree.timer.Elapsed.TotalMilliseconds;
        r.Iterations = tree.Iterations;
        r.MaxOpen = tree.MaxOpen;
        r.OpenAtEnd = tree.OpenAtEnd;
        r.MaxStored = tree.MaxStored;
        r.Aborted = tree.Aborted;

        var path = tree.ShortestPathToSolved();
        r.Solved = path != null;
        if (path != null) r.PathMoves = path.Count - 1;
        
        return r;
    }

    // output
    static string BuildCsv(List<RunResult> rs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Seed,Solved,Aborted,PathMoves,Iterations,MaxOpen,OpenAtEnd,MaxStored,TimeMs,Error");
        foreach (var r in rs)
        {
            sb.AppendLine(string.Join(",",
                r.Seed,
                r.Solved, r.Aborted, r.PathMoves, r.Iterations, r.MaxOpen,
                r.OpenAtEnd, r.MaxStored,
                r.TimeMs.ToString("0.###", CultureInfo.InvariantCulture), r.Error));
        }
        return sb.ToString();
    }

    static string BuildSummary(List<RunResult> rs, string method, string heuristic, int count,
                               int startSeed, int w, int h, float chance, double totalMs)
    {
        var ok = rs.Where(r => r.Error == "").ToList();
        var solved = ok.Where(r => r.Solved).ToList();
        var inv = CultureInfo.InvariantCulture;

        var sb = new StringBuilder();
        sb.AppendLine("Batch test summary");
        sb.AppendLine("==================");
        sb.AppendLine($"Algorithm:       {method}");
        sb.AppendLine($"Heuristic:       {heuristic}");
        sb.AppendLine($"Board:           {w}x{h}, fire chance {chance.ToString(inv)}");
        sb.AppendLine($"Boards:          {count}");
        sb.AppendLine($"Seed:            {startSeed}");
        sb.AppendLine($"Date:            {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total wall time: {totalMs.ToString("0.#", inv)} ms");
        sb.AppendLine();
        sb.AppendLine($"Solved:          {solved.Count} / {count} ({Pct(solved.Count, count)})");
        sb.AppendLine($"No solution:     {ok.Count(r => !r.Solved && !r.Aborted)}");
        sb.AppendLine($"Aborted (limit): {ok.Count(r => r.Aborted)}");
        sb.AppendLine($"Errors:          {rs.Count - ok.Count}");
        sb.AppendLine();

        sb.AppendLine("Metric (all non-error boards)");
        Stat(sb, "Iterations", ok.Select(r => (double)r.Iterations));
        Stat(sb, "MaxOpen",    ok.Select(r => (double)r.MaxOpen));
        Stat(sb, "MaxStored",  ok.Select(r => (double)r.MaxStored));
        Stat(sb, "TimeMs",     ok.Select(r => r.TimeMs));
        sb.AppendLine();

        sb.AppendLine("Metric (solved boards only)");
        Stat(sb, "PathMoves",  solved.Select(r => (double)r.PathMoves));
        Stat(sb, "Iterations", solved.Select(r => (double)r.Iterations));
        Stat(sb, "MaxStored",  solved.Select(r => (double)r.MaxStored));
        Stat(sb, "TimeMs",     solved.Select(r => r.TimeMs));
        return sb.ToString();
    }

    //???
    static string Pct(int a, int b) =>
        b == 0 ? "n/a" : (100.0 * a / b).ToString("0.#", CultureInfo.InvariantCulture) + "%";

    static void Stat(StringBuilder sb, string name, IEnumerable<double> values)
    {
        var v = values.OrderBy(x => x).ToList();
        if (v.Count == 0) { sb.AppendLine($"  {name,-11} n/a"); return; }
        double mean = v.Average();
        double median = v.Count % 2 == 1 ? v[v.Count / 2] : (v[v.Count / 2 - 1] + v[v.Count / 2]) / 2.0;
        double sd = Math.Sqrt(v.Sum(x => (x - mean) * (x - mean)) / v.Count);
        string f(double d) => d.ToString("0.##", CultureInfo.InvariantCulture);
        sb.AppendLine($"  {name,-11} mean {f(mean)}  median {f(median)}  sd {f(sd)}  min {f(v[0])}  max {f(v[^1])}");
    }

    static void Write(string path, string text)
    {
        using var f = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
        if (f == null)
            throw new System.IO.IOException("Cannot write " + path + ": " + Godot.FileAccess.GetOpenError());
        f.StoreString(text);
    }
}