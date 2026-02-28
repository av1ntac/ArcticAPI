using ArcticApi.Logic.Abstractions;
using ArcticApi.Model;

namespace ArcticApi.Logic.Services;

public sealed class PlayerService(IPlayerRepository repository)
{
    public Task<IReadOnlyCollection<Player>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<Player?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, cancellationToken);

    public Task<Player> UpsertAsync(Player player, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(player.Name);
        ArgumentOutOfRangeException.ThrowIfNegative(player.Health);
        return repository.UpsertAsync(player, cancellationToken);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);
}
