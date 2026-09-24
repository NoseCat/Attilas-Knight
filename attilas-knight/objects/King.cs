using Godot;
using System;

public partial class King : Node2D
{
	public Vector2I grid_pos;
	
	//graphics
	public void SetSize(Vector2 size)
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		var textureSize = sprite.Texture.GetSize();
		var currentSize = textureSize * sprite.Scale;
		sprite.Scale = size / textureSize;
	}
	
	public void set_pos(Vector2I new_pos)
	{
		grid_pos = new_pos;
	}
}
