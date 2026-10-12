using Godot;
using System;

public partial class Piece : Node2D
{
	public Vector2I grid_pos;
	
	//graphics
	public void SetSize(Vector2 size)
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		var textureSize = sprite.Texture.GetSize();
		sprite.Scale = size / textureSize;
	}
	
	public void SetPos(Vector2I new_pos)
	{
		Position = new_pos * GetNode<Field>("../Field").TileSize;
	}
}
