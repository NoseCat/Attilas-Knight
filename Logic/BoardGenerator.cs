using Godot;

public static class BoardGenerator
{
    public static State Generate(int seed, int width, int height, float fireChance)
    {
        var rng = new RandomNumberGenerator { Seed = (ulong)seed };

        var knight = new Vector2I(rng.RandiRange(0, width - 1), rng.RandiRange(0, height - 1));
        var king   = new Vector2I(rng.RandiRange(0, width - 1), rng.RandiRange(0, height - 1));

        var state = new State(new Vector2I(width, height), king, knight);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (new Vector2I(x, y) == knight || new Vector2I(x, y) == king) continue; //skip special
                if (rng.Randf() < fireChance) state.Cells[x, y].Walkable = false;
            }
        return state;
    }
}
