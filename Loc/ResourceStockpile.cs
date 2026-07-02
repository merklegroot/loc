namespace Loc;

public sealed class ResourceStockpile
{
  private readonly Dictionary<ResourceType, int> _counts = new();

  public int Get(ResourceType type) => _counts.GetValueOrDefault(type);

  public void Add(ResourceType type, int amount)
  {
    if (amount <= 0) return;
    _counts[type] = Get(type) + amount;
  }

  public bool CanSpend(IReadOnlyDictionary<ResourceType, int> cost)
  {
    foreach (var (type, amount) in cost)
    {
      if (Get(type) < amount) return false;
    }
    return true;
  }

  public bool Spend(IReadOnlyDictionary<ResourceType, int> cost)
  {
    if (!CanSpend(cost)) return false;
    foreach (var (type, amount) in cost)
    {
      _counts[type] = Get(type) - amount;
      if (_counts[type] == 0) _counts.Remove(type);
    }
    return true;
  }

  public void Absorb(ResourceStockpile other)
  {
    foreach (var (type, amount) in other._counts.ToList())
    {
      Add(type, amount);
    }
    other._counts.Clear();
  }

  public IEnumerable<(ResourceType Type, int Amount)> Enumerate() =>
    _counts.OrderBy(kv => kv.Key).Select(kv => (kv.Key, kv.Value));

  public ResourceStockpile Clone()
  {
    var copy = new ResourceStockpile();
    foreach (var (type, amount) in _counts)
    {
      copy._counts[type] = amount;
    }
    return copy;
  }
}
