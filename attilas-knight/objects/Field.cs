using Godot;
public partial class Field : Node2D
{
	public Vector2I GridSize = new Vector2I(10, 10);
	public Vector2 SpaceSize = new Vector2(0, 0);

	private PackedScene _tileScene;
	public Tile[,] tiles;
	Vector2 TileSize;

	public Horse horse;
	public King king;
	public override void _Ready()
	{
		_tileScene = GD.Load<PackedScene>("res://objects/tile.tscn");
		horse = GetNode<Horse>("../Horse");
		king = GetNode<King>("../King");
	}

//field creation
	public void Create(int size_x, int size_y)
	{
		Reset();

		GridSize = new Vector2I(size_x, size_y);

		tiles = new Tile[GridSize.X, GridSize.Y];

		TileSize = SpaceSize / GridSize;
		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				//data
				var tile = _tileScene.Instantiate<Tile>();
				AddChild(tile);
				tiles[x, y] = tile;

				//create
				var TilePos = new Vector2(x * TileSize.X, y * TileSize.Y);
				tile.Create(TilePos, TileSize);

				//graphic
				if ((x + y) % 2 == 0) tile.dark = true;
				tile.setLabel(x.ToString() + " " + y.ToString());
				tile.clearColor();
			}
		}
	}

	public void SetRandOnFire(float chance, int seed)
	{
		var rng = new RandomNumberGenerator();
		rng.Seed = (ulong)seed;
		//rng.Randomize();

		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				tiles[x, y].setClear();
				if (rng.RandiRange(0, 100) <= chance * 100) //100 should be rand precision
				{
					tiles[x, y].setFire();
				}
			}
		}
	}

	public void Reset()
	{
		Position = new Vector2(0, 0);

		foreach (Node child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}
	}

//helpers
	public Tile get_tile(Vector2I pos)
	{
		return tiles[pos.X, pos.Y];
	}

	public Vector2I find_selected()
	{
		for (int y = 0; y < GridSize.Y; y++)
			for (int x = 0; x < GridSize.X; x++)
				if(tiles[x,y].selected)
					return new Vector2I(x,y);
		return new Vector2I(-1,-1);
	}

	[Signal]
	public delegate void TilePressedEventHandler(Vector2I gridPos);

	public override void _Process(double delta)
	{
		//GD.Print(find_selected());
		horse.Position = horse.grid_pos * TileSize;
		horse.SetSize(TileSize);

		king.Position = king.grid_pos * TileSize;
		king.SetSize(TileSize);

		highlight_legal();

		if (Input.IsActionJustPressed("LMB"))
		{
			var pos = find_selected();
			if (pos.X != -1)
			{
				EmitSignal(SignalName.TilePressed, pos);

				var prev_pos = horse.grid_pos;
				bool move_success = try_move(pos);
				if(move_success)
				{
					get_tile(prev_pos).setBlock();
					clearColor();
				}
			}
		}
	}

	public void clearColor()
	{
		for (int y = 0; y < GridSize.Y; y++)
			for (int x = 0; x < GridSize.X; x++)
				tiles[x,y].clearColor();
	}

	public int legal_count()
	{
		var count = 0;
		for (int y = 0; y < GridSize.Y; y++)
			for (int x = 0; x < GridSize.X; x++)
				if(tiles[x,y].walkable && horse.Legal(new Vector2I(x,y)))
					count++;
		return count;
	}

	public void highlight_legal()
	{
		for (int y = 0; y < GridSize.Y; y++)
			for (int x = 0; x < GridSize.X; x++)
				if(tiles[x,y].walkable && horse.Legal(new Vector2I(x,y)))
					tiles[x,y].setColor(tiles[x,y].SelectColor);
	}

//horse control
	public bool check_move(Vector2I new_pos)
	{
		if(new_pos.X >= GridSize.X || new_pos.Y >= GridSize.Y || 
		new_pos.X < 0 || new_pos.Y < 0)
			return false;
		
		if(!get_tile(new_pos).walkable)
			return false;
		
		return horse.Legal(new_pos);
	}

	public bool try_move(Vector2I new_pos)
	{
		if(check_move(new_pos))
		{
			horse.set_pos(new_pos);	
			return true;
		}
		return false;
	}


}
