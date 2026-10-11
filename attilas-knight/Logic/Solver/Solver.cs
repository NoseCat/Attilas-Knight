using System;
using System.Collections.Generic;

public class Solver
{

    public MutationTree GetTree(string method, State initial)
    {
        //if initial.IsSolved
        
        switch (method)
        {
            case "BFS":
                return BFS(initial);
            case "DFS":
                return DFS(initial);
            case "Greedy":
                return Greedy(initial);
            case "Astar":
                return Astar(initial);
            default:
                throw new ArgumentException("Unknown method: " + method);
        }
    }

//Heuristics
    public static Func<State, int> h = Heuristics.Zero;

    public static void SetHeuristic(string name)
    {
        h = name switch
        {
            "Zero"      => Heuristics.Zero,
            "Manhattan" => Heuristics.Manhattan,
            _ => throw new ArgumentException("Unknown heuristic: " + name)
        };
    }
    
//Algorithms
//Contain a lot of repeating code, but I decided to leave it so its easy to see the difference

public static readonly Dictionary<string, Func<State, MutationTree>> Algorithms = new()
{
    ["BFS"]    = s => BFS(s),
    ["DFS"]    = s => DFS(s),
    ["Greedy"] = Greedy,
    ["Astar"]     = Astar,
};

   public static MutationTree BFS(State initial, bool full = false)
    {
        var tree = new MutationTree(initial);
        var queue = new Queue<State>();
        queue.Enqueue(initial);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if(cur.IsSolved() && !full) break;

            foreach (var move in cur.getLegalMoves())
            {
                var next = cur.moveKnight(move);
                if (tree.Visit(cur, next)) //true if visited for the first time
                    queue.Enqueue(next);
            }
        }
        return tree;
    }

    public static MutationTree DFS(State initial, bool full = false)
    {
        var tree = new MutationTree(initial);
        var stack = new Stack<State>();
        stack.Push(initial);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
        
            if(cur.IsSolved() && !full) break;

            foreach (var move in cur.getLegalMoves())
            {
                var next = cur.moveKnight(move);
                if (tree.Visit(cur, next))
                    stack.Push(next);
            }
        }
        return tree;
    }   
    
    public static MutationTree Greedy(State initial)
    {
        var tree = new MutationTree(initial);
        var queue = new PriorityQueue<State, int>();
        queue.Enqueue(initial, h(initial));

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur.IsSolved()) break;

            foreach (var move in cur.getLegalMoves())
            {
                var next = cur.moveKnight(move);
                if (tree.Visit(cur, next))
                {
                    int priority = h(next);
                    queue.Enqueue(next, priority);
                }
            }
        }
        return tree;
    }

    public static MutationTree Astar(State initial)
    {
        var tree = new MutationTree(initial);
        var queue = new PriorityQueue<State, int>();
        queue.Enqueue(initial, h(initial) + 0); //g = 0 for initial

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur.IsSolved()) break; //goal can be found earlier and sit lower in priority. Unlike with other, its important to check this on pop

            var g = tree.DistanceTo(cur);

            foreach (var move in cur.getLegalMoves())
            {
                var next = cur.moveKnight(move);
                if (tree.Visit(cur, next))
                {
                    int priority = h(next) + (g + 1);
                    queue.Enqueue(next, priority);
                }
            }
        }
        return tree;
    }
}