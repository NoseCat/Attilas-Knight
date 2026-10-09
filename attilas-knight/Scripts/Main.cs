using System.Runtime.Serialization;
using Godot;

public partial class Main : Node2D
{
	Field field;
	Label stateLabel;
	LineEdit seedInput, chanceFireInput, xInput, yInput;

	// placement toggles
	bool placeFire = false;
	bool placeKnight = false;
	bool placeKing = false;
	bool editMode => placeFire || placeKnight || placeKing;

	//saveload
	FileDialog loadDialog;
	const string SaveDir = "user://saves";

	public override void _Ready()
	{
		field = GetNode<Field>("FieldOrigin/Field");
		stateLabel = GetNode<Label>("State");
		seedInput = GetNode<LineEdit>("Seed");
		chanceFireInput = GetNode<LineEdit>("ChanceFire");
		xInput = GetNode<LineEdit>("X");
		yInput = GetNode<LineEdit>("Y");

		OnGeneratePressed();

		field.TilePressed += OnTilePressed;

		//saveload
		DirAccess.MakeDirRecursiveAbsolute(SaveDir);

		loadDialog = new FileDialog
		{
			FileMode = FileDialog.FileModeEnum.OpenFile,
			Access = FileDialog.AccessEnum.Filesystem,   // any file, anywhere
			Filters = new[] { "*.json ; Board files", "* ; All files" },
			CurrentDir = ProjectSettings.GlobalizePath(SaveDir),
			Title = "Load board"
		};
		loadDialog.FileSelected += OnLoadFileSelected;
		AddChild(loadDialog);
	}

	public override void _Process(double delta)
	{
		stateLabel.Text = "Turn: " + field.Path.Count.ToString();
		stateLabel.Text += "\n";
		stateLabel.Text += "Solved: " + field.Path[field.Path.Count - 1].IsSolved();
	}

	//Buttons 
	void OnGeneratePressed()
	{
		int seed = int.Parse(seedInput.Text);
		var rng = new RandomNumberGenerator();
		rng.Seed = (ulong)seed;
		float chance = float.Parse(chanceFireInput.Text);
		var X = int.Parse(xInput.Text);
		var Y = int.Parse(yInput.Text);

		field.Create(X, Y);
		field.SetRandKnight(rng);
		field.SetRandKing(rng);
		field.SetRandOnFire(chance, rng);
		field.Reset();
	}

	//Interaction
	void OnTilePressed(Vector2I gridPos)
	{
		if(!editMode)
			field.KnightTryMove(gridPos);
		if (placeFire)
			field.SetFire(gridPos);
		if (placeKnight)
			field.SetKnight(gridPos);
		if (placeKing)
			field.SetKing(gridPos);
	}

	public void _on_fire_manual_toggled(bool toggled_on)
	{
		placeFire = toggled_on;
		GD.Print(toggled_on);
	}
	public void _on_knight_manual_toggled(bool toggled_on)
	{
		placeKnight = toggled_on;
	}
	public void _on_king_manual_toggled(bool toggled_on)
	{
		placeKing = toggled_on;
	}
	public void _on_rand_seed_pressed()
	{
		var rng = new RandomNumberGenerator();
		seedInput.Text = rng.RandiRange(0, 10000).ToString();
	}

	public void _on_back_pressed()
	{
		field.ReverseLastMove();
	}

	public void _on_reset_pressed()
	{
		field.Reset();
	}
	public void _on_animate_pressed() => field.Animate();

	//save load
	public void _on_save_pressed()
	{
		string path = $"{SaveDir}/{seedInput.Text.Trim()}.json";
		using var f = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
		if (f == null)
		{
			GD.PrintErr("Save failed: ", Godot.FileAccess.GetOpenError());
			return;
		}
		f.StoreString(field.BoardState.ToJson());
		GD.Print("Saved to ", ProjectSettings.GlobalizePath(path));
	}

	public void _on_load_pressed()
	{
		loadDialog.PopupCenteredRatio(0.6f);
	}

	void OnLoadFileSelected(string path)
	{
		try
		{
			using var f = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
			if (f == null)
				throw new System.IO.IOException(Godot.FileAccess.GetOpenError().ToString());

			State loaded = State.FromJson(f.GetAsText());
			field.LoadState(loaded);

			xInput.Text = loaded.Size.X.ToString();
			yInput.Text = loaded.Size.Y.ToString();
		}
		catch (System.Exception e)
		{
			GD.PrintErr("Load failed: ", e.Message);
		}
	}

	//solver
	void OnSolveBFSPressed() => Solve("BFS");
	void OnSolveDFSPressed() => Solve("DFS");

	void Solve(string method)
	{
		var tree = new Solver().GetTree(method, field.BoardState);
		var path = tree.ShortestPathToSolved();
		if (path == null)
		{
			GD.Print(method + ": no solution");
			return;
		}
		field.ShowPath(path);
	}
}
