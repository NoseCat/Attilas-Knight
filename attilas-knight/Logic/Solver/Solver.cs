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
            default:
                throw new ArgumentException("Unknown method: " + method);
        }
    }
    
   public static MutationTree BFS(State initial, bool full = false)
    {
        var tree = new MutationTree(initial);
        var queue = new Queue<State>();
        queue.Enqueue(initial);

        while (queue.Count > 0)
        {
            if(tree.IsSolved && !full)
                break;

            var cur = queue.Dequeue();

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
            if(tree.IsSolved && !full)
                break;

            var cur = stack.Pop();

            foreach (var move in cur.getLegalMoves())
            {
                var next = cur.moveKnight(move);

                if (tree.Visit(cur, next))
                    stack.Push(next);
            }
        }
        return tree;
    }
}