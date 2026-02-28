namespace ArcticApi.Model;

public sealed class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public int Health { get; set; }
    public List<Knowledge> Knowledge { get; set; } = [];
}
