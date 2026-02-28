using ArcticApi.Logic.Abstractions;
using ArcticApi.Logic.Services;
using ArcticApi.Model;
using Xunit;

namespace ArcticApi.Tests;

public class PlayerServiceTests
{
    [Fact]
    public async Task UpsertAsync_StoresPlayerInRepository()
    {
        var repository = new InMemoryPlayerRepository();
        var service = new PlayerService(repository);

        var player = new Player { Name = "Aria", Health = 100 };
        await service.UpsertAsync(player);

        var stored = await service.GetByIdAsync(player.Id);
        Assert.NotNull(stored);
        Assert.Equal("Aria", stored.Name);
        Assert.Equal(100, stored.Health);
    }

    [Fact]
    public async Task UpsertAsync_ThrowsForNegativeHealth()
    {
        var repository = new InMemoryPlayerRepository();
        var service = new PlayerService(repository);

        var player = new Player { Name = "Aria", Health = -1 };
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.UpsertAsync(player));
    }

    private sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<string, Player> _items = [];

        public Task<IReadOnlyCollection<Player>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyCollection<Player>)_items.Values.ToList());

        public Task<Player?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(id));

        public Task<Player> UpsertAsync(Player player, CancellationToken cancellationToken = default)
        {
            _items[player.Id] = player;
            return Task.FromResult(player);
        }

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Remove(id));
    }
}
