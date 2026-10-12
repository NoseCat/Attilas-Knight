using Godot;
using System.Collections.Generic;
public partial class Field : Node2D
{
    public Piece knight;
	public Piece king;

	public static readonly Color ClearColor = new(0.85f, 0.85f, 0.80f);
	public static readonly float Tint = 0.25f;
	public static readonly float SelectTint = 0.3f;
	public static readonly Color FireColor = new(0.95f, 0.40f, 0.10f);
	public static readonly Color BlockedColor = new(0.25f, 0.25f, 0.28f);
	public static readonly Color LegalColor = new(0.2f, 0.9f, 0.3f, 0.45f);
	public static readonly Color GoalColor = new(0.95f, 0.90f, 0.45f);

    //node manipulation
    private void CreateNodes()
	{
		SpaceSize = GetNode<Node2D>("../FieldEdge").Position;
		TileSize = SpaceSize / BoardState.Size;
		tiles = new Tile[BoardState.Size.X, BoardState.Size.Y];

		for (int y = 0; y < BoardState.Size.Y; y++)
			for (int x = 0; x < BoardState.Size.X; x++)
			{
				var tile = _tileScene.Instantiate<Tile>();
				AddChild(tile);
				tiles[x, y] = tile;
				tile.Create(new Vector2I(x, y), TileSize);
				tile.Pressed += OnTilePressed;
			}
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

    //Visual
    public void UpdateView()
    {
        knight.SetPos(Last.Knight);
		UpdateCellColors();
    }

	private void UpdateCellColors(State last = null)
	{
		last ??= Last;

		//layer 1: Cells
		for (int y = 0; y < BoardState.Size.Y; y++)
			for (int x = 0; x < BoardState.Size.X; x++)
			{
				float curTint = 0f;
				if ((x + y) % 2 == 0) curTint = Tint;
				if (BoardState.Cells[x, y].Walkable)
					tiles[x, y].setColor(ClearColor.Lerp(Colors.Black, curTint));
				if (!BoardState.Cells[x, y].Walkable)
					tiles[x, y].setColor(FireColor.Lerp(Colors.Black, curTint));
				if (!last.Cells[x, y].Walkable && BoardState.Cells[x, y].Walkable)
					tiles[x, y].setColor(BlockedColor.Lerp(Colors.Black, curTint));
			}

		//layer 2: QoL
		if (!last.KingReached)
			tiles[BoardState.King.X, BoardState.King.Y].setColor(GoalColor);
		else
			tiles[BoardState.Start.X, BoardState.Start.Y].setColor(GoalColor);
		foreach (Vector2I move in last.getLegalMoves())
			tiles[move.X, move.Y].setColor(LegalColor);
	}

    // Animation
	int animId = 0;
	[Export] public float delay = 0.25f;
	public async void Animate()
	{
		var original = Path;
		var path = new List<State>(Path); // snapshot
		int id = ++animId; // starting a new animation cancels the old one

		knight.SetPos(path[0].Knight);
		UpdateCellColors(path[0]);

		for (int i = 1; i < path.Count; i++)
		{
			await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);

			// abort if restarted, or if Reset/Create/Load/Back/a move changed Path meanwhile
			if (id != animId || Path != original || Path.Count != path.Count)
				return;

			knight.SetPos(path[i].Knight);
			UpdateCellColors(path[i]);
		}
	}

}