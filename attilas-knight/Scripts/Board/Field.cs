using Godot;
using System.Collections.Generic;
public partial class Field : Node2D
{
	//data
	public State BoardState = null;
	public List<State> Path = new();

	//visual
	public Vector2 SpaceSize = new Vector2(0, 0);
	private PackedScene _tileScene;
	public Tile[,] tiles;
	public Vector2 TileSize;

	public Piece knight;
	public Piece king;

	static readonly Color ClearColor = new(0.85f, 0.85f, 0.80f);
	static readonly float Tint = 0.25f;
	static readonly Color FireColor = new(0.95f, 0.40f, 0.10f);
	static readonly Color BlockedColor = new(0.25f, 0.25f, 0.28f);
	static readonly Color LegalColor = new(0.2f, 0.9f, 0.3f, 0.45f);


	public override void _Ready()
	{
		_tileScene = GD.Load<PackedScene>("res://objects/tile.tscn");
		knight = GetNode<Piece>("../Horse");
		king = GetNode<Piece>("../King");
	}

//base
	public void Create(int size_x, int size_y)
	{
		Clear();

		BoardState = new State(new Vector2I(size_x, size_y));

		SpaceSize = GetNode<Node2D>("../FieldEdge").Position;
		TileSize = SpaceSize / BoardState.Size;
		tiles = new Tile[BoardState.Size.X, BoardState.Size.Y];

		for (int y = 0; y < BoardState.Size.Y; y++)
			for (int x = 0; x < BoardState.Size.X; x++)
			{
				var tile = _tileScene.Instantiate<Tile>();
				AddChild(tile);
				tiles[x, y] = tile;
				tile.Create(new Vector2I(x,y), TileSize);
				tile.Pressed += OnTilePressed;
			}
		
		Reset();
	}

	public void Clear()
	{
		Position = new Vector2(0, 0);

		foreach (Node child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}
	}

	public void Reset()
	{
		Path = new List<State>();
		Path.Add(BoardState);
		knight.SetPos(BoardState.Knight);
		knight.SetSize(TileSize);
		king.SetPos(BoardState.King);
		king.SetSize(TileSize);
		UpdateCellColors();
	}

//Visual
	public void UpdateCellColors(State last = null)
	{	
		last ??= Path[Path.Count - 1];

		//layer 1
		for (int y = 0; y < BoardState.Size.Y; y++)
			for (int x = 0; x < BoardState.Size.X; x++)
			{
				float curTint = 0f;
				if ((x + y) % 2 == 0) curTint = Tint;
				if(BoardState.Cells[x,y].Walkable)
					tiles[x,y].setColor(ClearColor.Lerp(Colors.Black, curTint));
				if(!BoardState.Cells[x,y].Walkable)
					tiles[x,y].setColor(FireColor.Lerp(Colors.Black, curTint));
				if(!last.Cells[x, y].Walkable && BoardState.Cells[x,y].Walkable)
					tiles[x,y].setColor(BlockedColor.Lerp(Colors.Black, curTint));
			}
		
		//layer 2
		foreach(Vector2I move in last.getLegalMoves())
			tiles[move.X, move.Y].setColor(LegalColor);

		//layer 3
		for (int y = 0; y < BoardState.Size.Y; y++)
			for (int x = 0; x < BoardState.Size.X; x++)
			{
				//highlight selected if tiles[x,y].selected 
			}
	}

	// public override void _Process(double delta)
	// {
	// 	//do anim
	// }

	//rands
	public void SetRandOnFire(float chance, RandomNumberGenerator rng)
	{
		for (int y = 0; y < BoardState.Size.Y; y++)
			for (int x = 0; x < BoardState.Size.X; x++)
				if (rng.RandiRange(0, 100) <= chance * 100) //100 should be rand precision
				{
					var pos = new Vector2I(x,y);
					if(pos == BoardState.Knight || pos == BoardState.King)
						continue;
					SetFire(x,y);
				}
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
	public void SetFire(Vector2I pos)
	{
		BoardState.Cells[pos.X, pos.Y].Walkable = false;
		Reset();
	}
	public void SetFire(int x, int y)
	{
		SetFire(new Vector2I(x,y));
	}

	public void SetKnight(Vector2I pos)
	{
		BoardState.Knight = pos;
		Reset();
	}
	public void SetKnight(int x, int y)
	{
		SetKnight(new Vector2I(x,y));
	}
	public void SetKing(Vector2I pos)
	{
		BoardState.King = pos;
		Reset();
	}
	public void SetKing(int x, int y)
	{
		SetKing(new Vector2I(x,y));
	}
	public void SetPath(List<State> path) //rename
	{
		Path = path;
		knight.SetPos(path[path.Count - 1].Knight);
		UpdateCellColors();
	}

//Knight Move
	public void KnightTryMove(Vector2I new_pos)
	{
		State last = Path[Path.Count - 1];
		if (last.IsLegal(new_pos))
		{
			State Move = last.moveKnight(new_pos);
			Path.Add(Move);
			knight.SetPos(Move.Knight);
		}
		UpdateCellColors();
	}

	public void ReverseLastMove()
	{
		Path.RemoveAt(Path.Count - 1);
		if(Path.Count <= 0)
		{
			Reset();
			return;
		}
		State last = Path[Path.Count - 1];
		knight.SetPos(last.Knight);
		UpdateCellColors();
	}

//interaction
	[Signal]
	public delegate void TilePressedEventHandler(Vector2I gridPos);
	void OnTilePressed(Vector2I gridPos)
	{
		EmitSignal(SignalName.TilePressed, gridPos);
	}

// Animation
	int animId = 0;
	[Export] float delay = 0.25f;
	public async void Animate()
	{
		var original = Path;
		var path = new List<State>(Path); // snapshot
		int id = ++animId; // starting a new animation cancels the old one

		knight.SetPos(path[0].Knight);
		UpdateCellColors(path[0]);

		for (int i = 1; i < path.Count; i++)
		{
			await ToSignal(GetTree().CreateTimer(	delay), SceneTreeTimer.SignalName.Timeout);

			// abort if restarted, or if Reset/Create/Load/Back/a move changed Path meanwhile
			if (id != animId || Path != original || Path.Count != path.Count)
				return;

			knight.SetPos(path[i].Knight);
			UpdateCellColors(path[i]);
		}
	}

//SaveLoad
	public void LoadState(State loaded)
	{
		Create(loaded.Size.X, loaded.Size.Y);
		BoardState.Cells = loaded.Cells;
		BoardState.King = loaded.King;
		BoardState.Knight = loaded.Knight;
		Reset();
	}

}
