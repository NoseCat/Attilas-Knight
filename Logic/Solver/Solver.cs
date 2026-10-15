//Algorithms contain a lot of repeating code, but I decided to leave it 
//so its easy to see the difference

using System;
using System.Collections.Generic;

public class Solver
{
    public SearchTree GetTree(string method, State initial)
    {
        //if initial.IsSolved
        
        if (!Algorithms.TryGetValue(method, out var algorithm))
            throw new ArgumentException("Unknown method: " + method);
        return algorithm(this, initial);
    }

//Heuristics
    public Func<State, int> h = Heuristics.Zero;

    public void SetHeuristic(string name)
    {
        if (!Heuristics.All.TryGetValue(name, out var heuristic))
            throw new ArgumentException("Unknown heuristic: " + name);
        h = heuristic;
    }
    
//Algorithms
    public static readonly Dictionary<string, Func<Solver, State, SearchTree>> Algorithms = new()
    {
        ["BFS"]     = (solver, s) => solver.BFS(s),
        ["DFS"]     = (solver, s) => solver.DFS(s),
        ["Greedy"]  = (solver, s) => solver.Greedy(s),
        ["Astar"]   = (solver, s) => solver.Astar(s),
    };

    public SearchTree BFS(State initial, bool full = false)
    {
        var tree = new SearchTree(initial);
        var queue = new Queue<State>();
        queue.Enqueue(initial);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if(cur.IsSolved() && !full) break;

            foreach (var next in tree.Expand(cur))
                queue.Enqueue(next);
        }
        return tree;
    }

    public SearchTree DFS(State initial, bool full = false)
    {
        var tree = new SearchTree(initial);
        var stack = new Stack<State>();
        stack.Push(initial);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
        
            if(cur.IsSolved() && !full) break;

            foreach (var next in tree.Expand(cur))
                stack.Push(next);
        }
        return tree;
    }   
    
    public SearchTree Greedy(State initial)
    {
        var tree = new SearchTree(initial);
        var queue = new PriorityQueue<State, int>();
        queue.Enqueue(initial, h(initial));

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur.IsSolved()) break;

            foreach (var next in tree.Expand(cur))
            {
                int priority = h(next);
                queue.Enqueue(next, priority);
            }
        }
        return tree;
    }

    public SearchTree Astar(State initial)
    {
        var tree = new SearchTree(initial);
        var queue = new PriorityQueue<State, int>();
        queue.Enqueue(initial, h(initial) + 0); //g = 0 for initial

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur.IsSolved()) break; //goal can be found earlier and sit lower in priority. Unlike with other, its important to check this on pop

            var g = tree.DistanceTo(cur);

            foreach (var next in tree.Expand(cur))
            {
                int priority = h(next) + (g + 1);
                queue.Enqueue(next, priority);
            }
        }
        return tree;
    }
}