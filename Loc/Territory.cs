namespace Loc;

public sealed class Territory
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required List<(int X, int Y)> Cells { get; init; }
    public HashSet<int> NeighborIds { get; } = [];

    public int OwnerId { get; set; } = -1;
    public ResourceType? Resource { get; set; }
    public bool HasHorse { get; set; }
    public bool HasWeapon { get; set; }
    public bool HasCity { get; set; }
    public int BoatCount { get; set; }
    public bool IsStockpile { get; set; }

    public (int X, int Y) Center
    {
        get
        {
            long sx = 0, sy = 0;
            foreach (var (x, y) in Cells)
            {
                sx += x;
                sy += y;
            }
            return ((int)(sx / Cells.Count), (int)(sy / Cells.Count));
        }
    }

    public (int X, int Y) GetDisplayCell(WorldMap map)
    {
        int[] dx = [0, 1, 0, -1];
        int[] dy = [-1, 0, 1, 0];
        var (avgX, avgY) = Center;

        (int X, int Y) best = Cells[0];
        int bestInland = -1;
        int bestCentrality = int.MinValue;

        foreach (var (x, y) in Cells)
        {
            int waterNeighbors = 0;
            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx < 0 || ny < 0 || nx >= map.Width || ny >= map.Height || map.IsWater[nx, ny])
                {
                    waterNeighbors++;
                }
            }

            int inland = 4 - waterNeighbors;
            int centrality = -((x - avgX) * (x - avgX) + (y - avgY) * (y - avgY));
            if (inland > bestInland || (inland == bestInland && centrality > bestCentrality))
            {
                bestInland = inland;
                bestCentrality = centrality;
                best = (x, y);
            }
        }

        return best;
    }
}
