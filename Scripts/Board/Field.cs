using Godot;
using System.Collections.Generic;
public partial class Field : Node2D
{
	//data
	public State BoardState = null;
	public List<State> Path = new();
	State Last => Path[^1];

	//visual
	public Vector2 SpaceSize = new Vector2(0, 0);
	private PackedScene _tileScene;
	public Tile[,] tiles;
	public Vector2 TileSize;

	public override void _Ready()
	{
		_tileScene = GD.Load<PackedScene>("res://Scenes/tile.tscn");
		knight = GetNode<Piece>("../Horse");
		king = GetNode<Piece>("../King");
	}

	//base
	public void Create(int size_x, int size_y)
	{
		Clear();
		BoardState = new State(new Vector2I(size_x, size_y));
		CreateNodes();
		Reset();
	}

	public void Reset()
	{
		Path = new List<State>();
		Path.Add(BoardState);
		knight.SetPos(BoardState.Knight);
		knight.SetSize(TileSize);
		king.SetPos(BoardState.King);
		king.SetSize(TileSize);
		UpdateView();
	}

	//public override void _Process(double delta)

	//rands
	public void SetRandOnFire(float chance, RandomNumberGenerator rng)
	{
		for (int y = 0; y < BoardState.Size.Y; y++)
			for (int x = 0; x < BoardState.Size.X; x++)
				if (rng.RandiRange(0, 100) <= chance * 100) //100 should be rand precision
				{
					var pos = new Vector2I(x, y);
					if (pos == BoardState.Knight || pos == BoardState.King)
						continue;
					SetFire(x, y, false);
				}
		Reset();
	}

	public void SetRandKnight(RandomNumberGenerator rng)
	{
		int x = rng.RandiRange(0, BoardState.Size.X - 1);
		int y = rng.RandiRange(0, BoardState.Size.Y - 1);

		SetKnight(x, y);
	}
	public void SetRandKing(RandomNumberGenerator rng)
	{
		int x = rng.RandiRange(0, BoardState.Size.X - 1);
		int y = rng.RandiRange(0, BoardState.Size.Y - 1);

		SetKing(x, y);
	}

	//Helpers
	public void SetFire(Vector2I pos, bool reset = true)
	{
		BoardState.Cells[pos.X, pos.Y].Walkable = false;
		if (reset) Reset();
	}
	public void SetFire(int x, int y, bool reset = true)
	{
		SetFire(new Vector2I(x, y));
	}

	public void SetKnight(Vector2I pos)
	{
		BoardState.Knight = pos;
		BoardState.Start = pos;
		Reset();
	}
	public void SetKnight(int x, int y)
	{
		SetKnight(new Vector2I(x, y));
	}
	public void SetKing(Vector2I pos)
	{
		BoardState.King = pos;
		Reset();
	}
	public void SetKing(int x, int y)
	{
		SetKing(new Vector2I(x, y));
	}
	public void SetPath(List<State> path) //rename
	{
		Path = path;
		UpdateView();
	}

	//Knight Move
	public void KnightTryMove(Vector2I new_pos)
	{
		State last = Path[Path.Count - 1];
		if (last.IsLegal(new_pos))
		{
			State Move = last.moveKnight(new_pos);
			Path.Add(Move);
		}
		UpdateView();
	}

	public void ReverseLastMove()
	{
		Path.RemoveAt(Path.Count - 1);
		if (Path.Count <= 0)
		{
			Reset();
			return;
		}
		UpdateView();
	}

	//interaction
	[Signal]
	public delegate void TilePressedEventHandler(Vector2I gridPos);
	void OnTilePressed(Vector2I gridPos)
	{
		EmitSignal(SignalName.TilePressed, gridPos);
	}

	//SaveLoad
	public void LoadState(State loaded)
	{
		Create(loaded.Size.X, loaded.Size.Y);
		BoardState.Cells = loaded.Cells;
		BoardState.King = loaded.King;
		BoardState.Knight = loaded.Knight;
		BoardState.Start = loaded.Start;
		Reset();
	}

}
