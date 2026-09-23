using Godot;
[Tool]
public partial class Field : Node2D
{
	public Vector2I GridSize = new Vector2I(10, 10);
	[Export] public Vector2 SpaceSize = new Vector2(0, 0);

	private PackedScene _tileScene;
	public Tile[,] tiles;
	public override void _Ready()
	{
		_tileScene = GD.Load<PackedScene>("res://objects/tile.tscn");

	}

	private bool _rebuild;
	[Export]
	public bool Rebuild
	{
		get => _rebuild;
		set
		{
			_rebuild = false;            // button-style toggle
			if (Engine.IsEditorHint())
			{
				_tileScene = GD.Load<PackedScene>("res://objects/tile.tscn");
				SpaceSize = GetNode<Node2D>("../FieldEdge").Position;
				Create(10,10);
			}

		}
	}

	public void Create(int size_x, int size_y)
	{
		Reset();

		GridSize = new Vector2I(size_x, size_y);

		tiles = new Tile[GridSize.X, GridSize.Y];

		Vector2 TileSize = SpaceSize / GridSize;
		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				var tile = _tileScene.Instantiate<Tile>();
				AddChild(tile);
				tiles[x, y] = tile;

				var TilePos = new Vector2(x * TileSize.X, y * TileSize.Y);
				tile.Create(TilePos, TileSize);
				if ((x + y) % 2 == 0) tile.Darken();
			}
		}
	}

	public void SetRandOnFire(float chance)
	{
		var rng = new RandomNumberGenerator();
		rng.Randomize();

		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				tiles[x, y].setClear();
				if (rng.RandiRange(0, 100) < chance * 100) //100 should be rand precision
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
}
