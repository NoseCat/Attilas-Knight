//Main is intentionally left messy, this is an interface interaction file

using Godot;

public partial class Main : Node2D
{
	Field field;
	Label stateLabel;
	LineEdit seedInput, chanceFireInput, xInput, yInput;
	OptionButton algoSelect, heuristicSelect;
	HSlider speedSlider;
	Label speedLabel;
	
	// placement toggles
	bool placeFire = false;
	bool placeKnight = false;
	bool placeKing = false;
	bool placeClear = false;
	bool editMode => placeFire || placeKnight || placeKing || placeClear;

	//saveload
	FileDialog loadDialog;
	const string SaveDir = "user://saves";

	string stats = "";

	public override void _Ready()
	{
		field = GetNode<Field>("FieldOrigin/Field");
		
		seedInput = GetNode<LineEdit>("Seed");
		chanceFireInput = GetNode<LineEdit>("ChanceFire");
		xInput = GetNode<LineEdit>("X");
		yInput = GetNode<LineEdit>("Y");

		algoSelect	= GetNode<OptionButton>("algoSelect");
		foreach (var name in Solver.Algorithms.Keys) algoSelect.AddItem(name);
		heuristicSelect = GetNode<OptionButton>("heuristicSelect");
		foreach (var name in Heuristics.All.Keys) heuristicSelect.AddItem(name);

		stateLabel = GetNode<Label>("State");
		speedLabel = GetNode<Label>("speedLabel");
		speedSlider = GetNode<HSlider>("speedSlider");
		_on_speed_slider_value_changed((float)speedSlider.Value);

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
		stateLabel.Text += "King Reached: " + field.Path[field.Path.Count - 1].KingReached;
		stateLabel.Text += "\n";
		stateLabel.Text += "Solved: " + field.Path[field.Path.Count - 1].IsSolved();
		GetNode<Label>("Stats").Text = stats;
	}

	//Buttons 
	void OnGeneratePressed()
	{
		int seed = int.Parse(seedInput.Text);
		float chance = float.Parse(chanceFireInput.Text);
		var X = int.Parse(xInput.Text);
		var Y = int.Parse(yInput.Text);

		field.CreateRand(seed, X, Y, chance);

		stats = "";
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
		if (placeClear)
			field.SetClear(gridPos);
	}

	public void _on_fire_manual_toggled(bool toggled_on)
	{
		placeFire = toggled_on;
	}
	public void _on_knight_manual_toggled(bool toggled_on)
	{
		placeKnight = toggled_on;
	}
	public void _on_king_manual_toggled(bool toggled_on)
	{
		placeKing = toggled_on;
	}

	public void _on_clear_manual_toggled(bool toggled_on)
	{
		placeClear = toggled_on;
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

	void _on_speed_slider_value_changed(float value)
	{
		field.delay = 1.0f - value;
		speedLabel.Text = "Anim Speed: " +  value.ToString();
	}


	public void _on_reset_pressed()
	{
		field.Reset();
		stats = "";
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
		//stats = "";	
	}


	//solver
	void _on_solve_pressed()
	{
		Solver.SetHeuristic(heuristicSelect.GetItemText(heuristicSelect.Selected));
		Solve(algoSelect.GetItemText(algoSelect.Selected));
	}

	void Solve(string method)
	{
		var tree = new Solver().GetTree(method, field.BoardState);
		stats = method + "\n" + tree.getStats();
		GD.Print(stats);
		var path = tree.ShortestPathToSolved();
		if (path == null)
		{
			GD.Print(method + ": no solution");
			return;
		}
		field.SetPath(path);
	}
}
