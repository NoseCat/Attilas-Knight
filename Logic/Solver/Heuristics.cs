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

    public static int Manhattan(State s) //not exactly manhattan
    {
        var d = (s.Knight - s.getGoal()).Abs();
        return (int)Math.Ceiling((d.X + d.Y) / 3.0);
    }
}