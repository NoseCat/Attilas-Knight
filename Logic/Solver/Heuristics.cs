using System;
using System.Collections.Generic;

public static class Heuristics
{
    public static readonly Dictionary<string, Func<State, int>> All = new()
    {
        ["Zero"]      = Zero,
        ["Manhattan"] = Manhattan,
    };

    // h = 0
    public static int Zero(State s) => 0;

    //pure manhattan divided by 3 becase knight changes the position by 3 manhattan units 
    public static int Manhattan(State s)
    {
        var d = (s.Knight - s.getGoal()).Abs();
        return (int)Math.Ceiling((d.X + d.Y) / 3.0);
    }

    //Chebyshev /2 because knight covers 2 chebyshev units
    public static int Chebyshev(State s)
    {
        var d = (s.Knight - s.getGoal()).Abs();
        return (int)Math.Ceiling(Math.Max(d.X, d.Y)/2.0);
    }

    public static int Combined(State s)
    {
        var d = (s.Knight - s.getGoal()).Abs();
        
        int manhattan = (int)Math.Ceiling((d.X + d.Y) / 3.0);
        int chebyshev = (int)Math.Ceiling(Math.Max(d.X, d.Y)/2.0);
        int h = Math.Max(manhattan, chebyshev);

        // Every knight move flips the parity of (x+y), so the true move count
        // always has the same parity as dx+dy. If h has the wrong parity it can't
        // be exact, so the true cost is >= h+1 (which has the right parity).
        if (h % 2 != (d.X + d.Y) % 2) h++;

        return h;
    }
}