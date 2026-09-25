using Godot;
using System;

public partial class Horse : Node2D
{
	public Vector2I grid_pos = new Vector2I(0,0);
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void set_pos(Vector2I new_pos)
	{
		grid_pos = new_pos;
	}

	public bool Legal(Vector2I pos)
	{
		int dx = Mathf.Abs(pos.X - grid_pos.X);
		int dy = Mathf.Abs(pos.Y - grid_pos.Y);
		if((dx == 2 && dy == 1) || (dx == 1 && dy == 2))
			return true;
		return false;
	}

//graphics
	public void SetSize(Vector2 size)
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		var textureSize = sprite.Texture.GetSize();
		var currentSize = textureSize * sprite.Scale;
		sprite.Scale = size / textureSize;
	}
}
