using System.Collections.Generic;
using System;
using System.Threading;
using System.Diagnostics;
public class SearchTree
{
    public State Initial { get; }

    private readonly Dictionary<State, List<State>> parents = new();
    private readonly Dictionary<State, int> depth = new();

    public List<State> solved = new();


    //safety
    public int MaxIterations = 1_000_000;
    public bool Aborted { get; private set; }

    //stats
    public int Iterations { get; private set; }
    public int MaxOpen { get; private set; }
    public int MaxStored => depth.Count;
    public int OpenAtEnd => depth.Count - Iterations;
    public Stopwatch timer = Stopwatch.StartNew();

    public SearchTree(State initial)
    {
        Initial = initial;
        depth[initial] = 0;
        parents[initial] = new List<State>();
    }

//building
    /// <summary>
    /// Expands a state: returns every possible, yet unvisited state and
    /// expands itslef with them
    /// </summary>
    public List<State> Expand(State parent)
    {
        if (Iterations >= MaxIterations) { Aborted = true; return new List<State>(); }
        Iterations++;

        var fresh = new List<State>();
        foreach (var child in parent.getMutations())
        {
            if (Visit(parent, child)) fresh.Add(child); // only newly discovered states
        }
        return fresh;
    }

    // Returns true if child is new.
    private bool Visit(State parent, State child)
    {
        int d = depth[parent] + 1;
 
        //if new state (child)
        if (!depth.TryGetValue(child, out int existingDepth))
        {
            depth[child] = d;
            parents[child] = new List<State> { parent };
            if (child.IsSolved())
                solved.Add(child);

            MaxOpen = Math.Max(MaxOpen, depth.Count - Iterations);
            return true;
        }
        //if not new

        if (existingDepth == d)
            parents[child].Add(parent); // another equally short route

        return false;
    }

//queries
    public bool Contains(State s) => depth.ContainsKey(s);

    public int VisitedCount => depth.Count;
    public bool IsSolved => solved.Count != 0;

    /// Shortest distance to a state, or -1 if it was never reached.
    public int DistanceTo(State s) => depth.TryGetValue(s, out int d) ? d : -1;

    // null if unreachable. O(path length).
    public List<State> ShortestPathTo(State target = null)
    {
        if (target == null || !Contains(target)) return null;

        var path = new List<State> { target };
        var cur = target;
        while (parents[cur].Count > 0)
        {
            cur = parents[cur][0];
            path.Add(cur);
        }
        path.Reverse();
        return path;
    }

    public List<State> ShortestPathToSolved()
    {
        State Shortest = null;
        int shortestDepth = int.MaxValue;
        foreach(State cur in solved)
        {
            if(depth[cur] < shortestDepth)
            {
                shortestDepth = depth[cur];
                Shortest = cur;
            }
        }

        return ShortestPathTo(Shortest);
    }

    public string getStats()
    {
        return $"Iterations: {Iterations}\n" +
			$"Open: max {MaxOpen}, at end {OpenAtEnd}\n" +
			$"Max stored (O+C): {MaxStored}\n" + 
            $"Time (ms): {timer.ElapsedMilliseconds}";
    }
}