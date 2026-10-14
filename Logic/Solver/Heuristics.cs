using System;
using System.Collections.Generic;
using Godot;

public static class Heuristics
{
    public static readonly Dictionary<string, Func<State, int>> All = new()
    {
        ["Zero"] = Zero,
        ["Manhattan"] = Manhattan,
        ["Chebyshev"] = Chebyshev,
        ["Combined"] = Combined,
    };

    // h = 0
    public static int Zero(State s) => 0;

    public static int Manhattan(State s)
    {
        int h;
        if(!s.KingReached)
        {
            var d1 = (s.Knight - s.King).Abs();
            h = (int)Math.Ceiling((d1.X + d1.Y) / 3.0);
            var d2 = (s.King - s.Start).Abs();
            h += (int)Math.Ceiling((d2.X + d2.Y) / 3.0);
        }
        else
        {
            var d = (s.Knight - s.Start).Abs();
            h = (int)Math.Ceiling((d.X + d.Y) / 3.0);
        }
        return h;
    }

    //Chebyshev /2 because knight covers 2 chebyshev units
    public static int Chebyshev(State s)
    {
        int h;
        if(!s.KingReached)
        {
            var d1 = (s.Knight - s.King).Abs();
            h = (int)Math.Ceiling(Math.Max(d1.X, d1.Y)/2.0);
            var d2 = (s.King - s.Start).Abs();
            h += (int)Math.Ceiling(Math.Max(d2.X, d2.Y)/2.0);
        }
        else
        {
            var d = (s.Knight - s.Start).Abs();
            h = (int)Math.Ceiling(Math.Max(d.X, d.Y)/2.0);
        }
        return h;
    }

   public static int Combined(State s)
    {
        int h;
        if (!s.KingReached)
        {
            h = CombinedLeg(s.Knight, s.King) + CombinedLeg(s.King, s.Start);
        }
        else
        {
            h = CombinedLeg(s.Knight, s.Start);
        }
        return h;
    }

    static int CombinedLeg(Vector2I a, Vector2I b)
    {
        var d = (a - b).Abs();
        int manhattan = (int)Math.Ceiling((d.X + d.Y) / 3.0);
        int chebyshev = (int)Math.Ceiling(Math.Max(d.X, d.Y) / 2.0);
        int h = Math.Max(manhattan, chebyshev);

        // every knight move flips parity of (x+y), so the move count has the parity of dx+dy
        if (h % 2 != (d.X + d.Y) % 2) h++;
        return h;
    }

//stupid "get back to start" rule forces Heuristics to be cringe. I provided purer version below
/*
    //manhattan divided by 3 becase knight changes the position by 3 units
    public static int Manhattan(State s)
    {
      var d = (s.CurPos - s.Finish).Abs();
      return Math.Ceiling((d.X + d.Y) / 3.0);
    }

    //Chebyshev /2 because knight covers 2 units
    public static int Chebyshev(State s)
    {
      var d = (s.CurPos - s.Finish).Abs();
      return Math.Ceiling(Math,Max(d.X, d.Y) / 2.0);
    }

    public static int Combined(State s)
    {
        var d = (s.CurPos - s.Finish).Abs()
        
        int manhattan = Manhattan(s);
        int chebyshev = Chebyshev(s);
        int h = Math.Max(manhattan, chebyshev);

        // Every knight move flips the parity of (x+y), so the true move count
        // always has the same parity as dx+dy. If h has the wrong parity it can't
        // be exact, so the true cost is >= h+1 (which has the right parity).
        if (h % 2 != (d.X + d.Y) % 2) h++;

        return h;
    }
*/
}