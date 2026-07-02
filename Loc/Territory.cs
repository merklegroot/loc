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
}
