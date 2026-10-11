using Godot;
using System.Collections.Generic;
using System;
using System.Text.Json;
public struct Cell
{
    public Cell(bool walkable)
    {
        Walkable = walkable;
    }
    public Cell()
    {
        Walkable = true;
    }
    public bool Walkable {get; set;} = true;
}

public class State
{
    //static
    public Vector2I Size {get; private set;} //techically a hack that is unnecesarry for the state, the fix is too complex to be worth it
    private static readonly Vector2I[] KnightMoves =
	{
		new Vector2I( 2,  1),
		new Vector2I( 1,  2),
		new Vector2I(-1,  2),
		new Vector2I(-2,  1),
		new Vector2I(-2, -1),
		new Vector2I(-1, -2),
		new Vector2I( 1, -2),
		new Vector2I( 2, -1),
	};

    //data
	public Cell[,] Cells {get; set;} //dynamic
    public Vector2I Knight {get; set;} //dynamic
    public bool KingReached {get; set;} //dynamic, false by default
    public Vector2I King {get; set;} //static
    public Vector2I Start {get; set;} //static

    //base
    public State(Vector2I size)
    {
        Size = size;
        Cells = NewCells(size);
        King = new Vector2I(0,0);
        Knight = new Vector2I(0,0);
        Start = Knight;
    }
    public State(Vector2I size, Vector2I king, Vector2I knight)
    {
        Size = size;
        Cells = NewCells(size);
        King = king;
        Knight = knight;
        Start = knight;
    }

    public State(State other)
    {
        Size = other.Size;
        Cells = (Cell[,])other.Cells.Clone();
		King = other.King;
        Knight = other.Knight;
        Start = other.Start;
        KingReached = other.KingReached;
    }

    public State Clone() => new State(this);

    private static Cell[,] NewCells(Vector2I size)
    {
        var cells = new Cell[size.X, size.Y];
        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
                cells[x, y] = new Cell(true);
        return cells;
    }

    public bool Equals(State other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Knight != other.Knight || KingReached != other.KingReached 
        ||  Start != other.Start || King != other.King) //this line's comparisons are unnecessary for mutations
            return false;

        for (int x = 0; x < Size.X; x++)
            for (int y = 0; y < Size.Y; y++)
                if (Cells[x, y].Walkable != other.Cells[x, y].Walkable)
                    return false;
        return true;
    }
    public override bool Equals(object obj) => Equals(obj as State);

    public override int GetHashCode()
    {
        var h = new HashCode();
        h.Add(Knight);
        h.Add(KingReached);
        for (int x = 0; x < Size.X; x++)
            for (int y = 0; y < Size.Y; y++)
                h.Add(Cells[x, y].Walkable);
        return h.ToHashCode();
    }

    //mutate
    public State moveKnight(Vector2I to)
    {
        var newState = Clone();
        var from = Knight;
        if(IsLegal(to))
        {
            newState.blockCell(from);
            newState.Knight = to;
            if (to == King)
            {
                newState.KingReached = true;
                newState.unblockCell(Start);
            } 
        }

        return newState;
    }

    //checks
    private bool IsWalkable(Vector2I move)
    {
        //out of bounds
        if(move.X < 0 || move.Y < 0 || move.X >= Size.X || move.Y >= Size.Y)
            return false;

        //square blocked
        if(!getCell(move).Walkable)
            return false;

        return true;
    }

    public bool IsLegal(Vector2I move)
    {
        if(!IsWalkable(move))
            return false;

        int dx = Mathf.Abs(Knight.X - move.X);
		int dy = Mathf.Abs(Knight.Y - move.Y);
		if((dx == 2 && dy == 1) || (dx == 1 && dy == 2))
			return true;

        return false;
    }

    public bool IsSolved()
    {
        return KingReached && Knight == Start;
    }

    //helper
    public List<Vector2I> getLegalMovesFrom(Vector2I pos)
    {
        var list = new List<Vector2I> {};
        foreach (var move in KnightMoves)
        {
            var next = pos + move;
            if (IsWalkable(next))
                list.Add(next);
        }
        return list;
    }

    public List<Vector2I> getLegalMoves()
    {
        return getLegalMovesFrom(Knight);
    }

    private void blockCell(Vector2I pos)
    {
        Cells[pos.X, pos.Y].Walkable = false;
    }

    private void unblockCell(Vector2I pos)
    {
        Cells[pos.X, pos.Y].Walkable = true;
    }

    private Cell getCell(Vector2I pos)
    {
        return Cells[pos.X, pos.Y];
    }

    private Cell getCell(int X, int Y)
    {
        return Cells[X, Y];
    }

    //serialization
    public string ToJson()
    {
        var rows = new string[Size.Y];
        for (int y = 0; y < Size.Y; y++)
        {
            var chars = new char[Size.X];
            for (int x = 0; x < Size.X; x++)
                chars[x] = Cells[x, y].Walkable ? '.' : '#';
            rows[y] = new string(chars);
        }

        var data = new StateData
        {
            Width = Size.X,
            Height = Size.Y,
            King = new[] { King.X, King.Y },
            Knight = new[] { Knight.X, Knight.Y },
            Rows = rows
        };
        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    public static State FromJson(string json)
    {
        var d = JsonSerializer.Deserialize<StateData>(json);
        if (d == null || d.Width <= 0 || d.Height <= 0 || d.Rows == null || d.Rows.Length != d.Height
            || d.King is not { Length: 2 } || d.Knight is not { Length: 2 })
            throw new FormatException("Invalid board file");

        var s = new State(new Vector2I(d.Width, d.Height),
                        new Vector2I(d.King[0], d.King[1]),
                        new Vector2I(d.Knight[0], d.Knight[1]));

        for (int y = 0; y < d.Height; y++)
        {
            if (d.Rows[y].Length != d.Width)
                throw new FormatException("Invalid row length");
            for (int x = 0; x < d.Width; x++)
                s.Cells[x, y].Walkable = d.Rows[y][x] != '#';
        }
        return s;
    }
}

public class StateData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int[] King { get; set; }
    public int[] Knight { get; set; }
    public string[] Rows { get; set; }   // '.' = walkable, '#' = fire/blocked
}