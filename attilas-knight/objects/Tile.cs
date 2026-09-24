using Godot;
public partial class Tile : Node2D
{
	public bool walkable = true;

	public bool selected = false;

	ColorRect colorRect;
	Color color;
	public bool dark = false;
	[Export] Color BaseColor = new Color(0.8f, 0.8f, 1f, 1);
	[Export] public Color SelectColor = new Color(0.07f, 0.21f, 0.71f, 1);

	Label label;

	[Export] Color Fire = new Color(0.75f, 0.25f, 0, 1);
	[Export] Color Blocked = new Color(0.5f, 0.5f, 0.5f, 1);
	[Export] float tint = 0.25f;
	//[Export] Color light = new Color(1,1,1, 1);
	
	public override void _Ready()
	{
		label = GetNode<Label>("Label");
		colorRect = GetNode<ColorRect>("ColorRect");
	}

//Create
	public void Create(Vector2 pos, Vector2 size)
	{
		Position = pos;
		colorRect.Size = size;
		setClear();
	}

//Label
	public void setLabel(string str)
	{
		label.Text = str;
	}

//Logic
	public void setClear()
	{
		walkable = true;
		color = BaseColor;
		clearColor();
		//setColor(dark ? BaseColor : DarkColor);
	}

	public void setBlock()
	{
		walkable = false;
		color = Blocked;
		clearColor();
		//setColor(dark ? Blocked : Blocked.Lerp(Colors.Black, tint));
	}

	public void setFire()
	{
		walkable = false;
		color = Fire;
		clearColor();
		//setColor(dark ? Fire : Fire.Lerp(Colors.Black, tint));
	}

	public void Selected()
	{
		selected = true;
		setColor(color.Lerp(Colors.White, tint*2));
	}

	public void Unselected()
	{
		selected = false;
		clearColor();
	}

//Color control
	public void clearColor()
	{
		setColor(color);
	}
	public void setColor(Color color)
	{
		if (dark) color = color.Lerp(Colors.Black, tint);
		colorRect.Color = color;
	}
}
