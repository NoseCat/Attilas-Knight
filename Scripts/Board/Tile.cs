using Godot;
public partial class Tile : Node2D
{
	public bool selected = false;
	private Color color;

	ColorRect colorRect;
	Label label;

	Vector2I GridPos = new Vector2I(0,0);
	public override void _Ready()
	{
		label = GetNode<Label>("Label");
		colorRect = GetNode<ColorRect>("ColorRect");
	}

	public void Create(Vector2I pos, Vector2 size)
	{
		GridPos = pos;
		colorRect.Size = size;
		Position = GridPos * colorRect.Size;
	}

	public void setLabel(string str)
	{
		label.Text = str;
	}

	public Color getColor()
	{
		return color;
	}
	
	public void setColor(Color color_)
	{
		color = color_;
		colorRect.Color = color;
	}

	public void Selected()
	{
		selected = true;
		Color lighter = color.Lerp(Colors.White, Field.SelectTint);
		colorRect.Color = lighter; // hacky
	}
	public void Unselected()
	{
		selected = false;
		setColor(color);
	}

	[Signal]
	public delegate void PressedEventHandler(Vector2I gridPos);
	public void OnPressed(InputEvent @event)
	{
		if (@event.IsActionPressed("LMB"))
		{
			EmitSignal(SignalName.Pressed, GridPos);
		}
	}

}
