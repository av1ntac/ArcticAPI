using ArcticApi.Model;

namespace ArcticApi.Logic.Abstractions;

public interface IKnowledgeRepository
{
    Task<IReadOnlyCollection<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Knowledge?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Knowledge> UpsertAsync(Knowledge knowledge, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
