using ArcticApi.Model;

namespace ArcticApi.Logic.Abstractions;

public interface IPlayerRepository
{
    Task<IReadOnlyCollection<Player>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Player?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Player> UpsertAsync(Player player, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
