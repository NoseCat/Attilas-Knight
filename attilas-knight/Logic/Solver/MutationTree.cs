using System.Collections.Generic;
using System.Linq;

public class MutationTree
{
    public State Initial { get; }

    private readonly Dictionary<State, List<State>> parents = new();
    private readonly Dictionary<State, int> depth = new();

    public List<State> solved = new();

    public MutationTree(State initial)
    {
        Initial = initial;
        depth[initial] = 0;
        parents[initial] = new List<State>();
    }

    //building

    // Returns true if child is new.
    public bool Visit(State parent, State child)
    {
        int d = depth[parent] + 1;

        //if new state
        if (!depth.TryGetValue(child, out int existingDepth))
        {
            depth[child] = d;
            parents[child] = new List<State> { parent };
            if (child.IsSolved())
                solved.Add(child);
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
}