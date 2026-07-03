namespace Loc;

public static class MapGenerator
{
    private static readonly string[] NameParts =
    [
        "North", "South", "East", "West", "Upper", "Lower", "New", "Old",
        "Red", "Green", "High", "Low", "Lake", "River", "Coast", "Hill"
    ];

    public static WorldMap Generate(GameConfig config)
    {
        var rng = new Random(config.Seed);
        int width = 52;
        int height = 38;
        var isWater = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool edge = x < 2 || y < 2 || x >= width - 2 || y >= height - 2;
                bool nearCoast = x < 4 || y < 4 || x >= width - 4 || y >= height - 4;
                bool coastalNoise = nearCoast && !edge && rng.NextDouble() < config.WaterRatio;
                isWater[x, y] = edge || coastalNoise;
            }
        }

        RemoveEnclosedWater(isWater, width, height);

        var landCells = new List<(int X, int Y)>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!isWater[x, y]) landCells.Add((x, y));
            }
        }

        int territoryCount = Math.Clamp(config.TerritoryCount, 8, 28);
        var seeds = PickSeeds(landCells, territoryCount, rng);
        var labels = AssignVoronoi(width, height, isWater, seeds);

        var territories = BuildTerritories(labels, seeds, rng);
        AssignResources(territories, config, rng);
        ComputeNeighbors(territories, labels, width, height);

        return new WorldMap
        {
            Width = width,
            Height = height,
            IsWater = isWater,
            TerritoryGrid = labels,
            Territories = territories
        };
    }

    private static void RemoveEnclosedWater(bool[,] isWater, int width, int height)
    {
        var reachable = new bool[width, height];
        var queue = new Queue<(int X, int Y)>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!isWater[x, y]) continue;
                bool onMapEdge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                if (!onMapEdge) continue;
                reachable[x, y] = true;
                queue.Enqueue((x, y));
            }
        }

        int[] dx = [0, 1, 0, -1];
        int[] dy = [-1, 0, 1, 0];
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (!isWater[nx, ny] || reachable[nx, ny]) continue;
                reachable[nx, ny] = true;
                queue.Enqueue((nx, ny));
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (isWater[x, y] && !reachable[x, y])
                {
                    isWater[x, y] = false;
                }
            }
        }
    }

    private static List<(int X, int Y)> PickSeeds(List<(int X, int Y)> land, int count, Random rng)
    {
        var pool = land.ToList();
        var seeds = new List<(int X, int Y)>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = rng.Next(pool.Count);
            seeds.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return seeds;
    }

    private static int[,] AssignVoronoi(int width, int height, bool[,] isWater, List<(int X, int Y)> seeds)
    {
        var labels = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (isWater[x, y])
                {
                    labels[x, y] = -1;
                    continue;
                }

                int best = 0;
                int bestDist = int.MaxValue;
                for (int i = 0; i < seeds.Count; i++)
                {
                    int dx = x - seeds[i].X;
                    int dy = y - seeds[i].Y;
                    int dist = dx * dx + dy * dy;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = i;
                    }
                }
                labels[x, y] = best;
            }
        }
        return labels;
    }

    private static List<Territory> BuildTerritories(int[,] labels, List<(int X, int Y)> seeds, Random rng)
    {
        var cellsById = new Dictionary<int, List<(int X, int Y)>>();
        int w = labels.GetLength(0);
        int h = labels.GetLength(1);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                int id = labels[x, y];
                if (id < 0) continue;
                if (!cellsById.TryGetValue(id, out var list))
                {
                    list = [];
                    cellsById[id] = list;
                }
                list.Add((x, y));
            }
        }

        var territories = new List<Territory>();
        foreach (var (id, cells) in cellsById.OrderBy(kv => kv.Key))
        {
            string name = $"{NameParts[rng.Next(NameParts.Length)]} {NameParts[rng.Next(NameParts.Length)]}";
            territories.Add(new Territory
            {
                Id = id,
                Name = name,
                Cells = cells
            });
        }
        return territories;
    }

    private static void AssignResources(List<Territory> territories, GameConfig config, Random rng)
    {
        var pool = territories.OrderBy(_ => rng.Next()).ToList();
        var types = config.AvailableResources.ToList();
        int perType = Math.Max(1, pool.Count / (types.Count * 2));

        int idx = 0;
        foreach (var type in types)
        {
            for (int i = 0; i < perType && idx < pool.Count; i++, idx++)
            {
                pool[idx].Resource = type;
                if (type == ResourceType.Horses)
                {
                    pool[idx].HasHorse = true;
                }
            }
        }

        while (idx < pool.Count)
        {
            var type = types[rng.Next(types.Count)];
            pool[idx].Resource = type;
            if (type == ResourceType.Horses) pool[idx].HasHorse = true;
            idx++;
        }
    }

    private static void ComputeNeighbors(List<Territory> territories, int[,] labels, int width, int height)
    {
        var byId = territories.ToDictionary(t => t.Id);
        var pairs = new HashSet<(int, int)>();
        int[] dx = [0, 1, 0, -1];
        int[] dy = [-1, 0, 1, 0];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int a = labels[x, y];
                if (a < 0) continue;
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    int b = labels[nx, ny];
                    if (b < 0 || a == b) continue;
                    int lo = Math.Min(a, b);
                    int hi = Math.Max(a, b);
                    pairs.Add((lo, hi));
                }
            }
        }

        foreach (var (a, b) in pairs)
        {
            byId[a].NeighborIds.Add(b);
            byId[b].NeighborIds.Add(a);
        }
    }
}
