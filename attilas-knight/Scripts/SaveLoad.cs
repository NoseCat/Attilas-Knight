// using Godot;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;

// public static class SaveLoad
// {
// 	const string Dir = "user://saves/";

// 	public class Data
// 	{
// 		public int[] Size { get; set; }
// 		public int[] King { get; set; }
// 		public int[] Knight { get; set; }
// 		public string Cells { get; set; }          // locked state, row-major by x, '1' = walkable
// 		public List<int[]> Turns { get; set; }     // knight positions after the locked state
// 	}

// 	static string PathFor(string name) => Dir + (string.IsNullOrWhiteSpace(name) ? "quicksave" : name.Trim()) + ".json";

// 	public static bool Save(Field f, string name)
// 	{
// 		DirAccess.MakeDirRecursiveAbsolute(Dir);
// 		var c = f.Current;
// 		var data = new Data
// 		{
// 			Size = new[] { c.Size.X, c.Size.Y },
// 			King = new[] { c.King.X, c.King.Y },
// 			Knight = new[] { c.Knight.X, c.Knight.Y },
// 			Cells = f.CellsToString(),
// 			Turns = f.Path.Select(t => new[] { t.Knight.X, t.Knight.Y }).ToList(),
// 		};

// 		using var file = Godot.FileAccess.Open(PathFor(name), Godot.FileAccess.ModeFlags.Write);
// 		if (file == null) { GD.PushError("Save failed: " + Godot.FileAccess.GetOpenError()); return false; }
// 		file.StoreString(JsonSerializer.Serialize(data));
// 		return true;
// 	}

// 	public static bool Load(Field f, string name)
// 	{
// 		using var file = Godot.FileAccess.Open(PathFor(name), Godot.FileAccess.ModeFlags.Read);
// 		if (file == null) { GD.PushError("Load failed: " + Godot.FileAccess.GetOpenError()); return false; }

// 		Data d;
// 		try { d = JsonSerializer.Deserialize<Data>(file.GetAsText()); }
// 		catch (JsonException e) { GD.PushError("Bad save file: " + e.Message); return false; }

// 		if (d?.Size == null || d.King == null || d.Knight == null || d.Cells == null
// 			|| d.Cells.Length != d.Size[0] * d.Size[1])
// 		{
// 			GD.PushError("Bad save file: inconsistent data");
// 			return false;
// 		}

// 		f.Restore(
// 			new Vector2I(d.Size[0], d.Size[1]),
// 			new Vector2I(d.King[0], d.King[1]),
// 			new Vector2I(d.Knight[0], d.Knight[1]),
// 			d.Cells,
// 			(d.Turns ?? new()).Select(t => new Vector2I(t[0], t[1])));
// 		return true;
// 	}
// }