using Godot;
[Tool] 
public partial class Tile : Node2D
{
	bool walkable = true;

	ColorRect colorRect;
	bool dark = false;
	[Export] Color BaseColor = new Color(1, 1, 1, 1);
	[Export] Color DarkColor = new Color(0.07f, 0.21f, 0.71f, 1);

	[Export] Color Fire = new Color(0.75f, 0.25f, 0, 1);
	[Export] Color Blocked = new Color(0.5f, 0.5f, 0.5f, 1);
	[Export] float tint = 0.25f;

	public override void _Ready()
	{
		colorRect = GetNode<ColorRect>("ColorRect");
	}

	public void Create(Vector2 pos, Vector2 size)
	{
		Position = pos;
		colorRect.Size = size;
		setColor(BaseColor);
	}

	public void Darken()
	{
		dark = true;
		colorRect.Color = DarkColor;
	}

	public void setClear()
	{
		walkable = true;
		setColor(dark ? DarkColor : BaseColor);
	}

	public void setBlock()
	{
		walkable = false;
		setColor(Blocked);
	}

	public void setFire()
	{
		walkable = false;
		setColor(Fire);
	}

	public void setColor(Color color)
	{
		colorRect.Color = color;
		if (dark) colorRect.Color = colorRect.Color.Lerp(Colors.Black, tint); //non default color fallback
	}
}
