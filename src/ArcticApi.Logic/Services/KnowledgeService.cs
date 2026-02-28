using ArcticApi.Logic.Abstractions;
using ArcticApi.Model;

namespace ArcticApi.Logic.Services;

public sealed class KnowledgeService(IKnowledgeRepository repository)
{
    public Task<IReadOnlyCollection<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<Knowledge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, cancellationToken);

    public Task<Knowledge> UpsertAsync(Knowledge knowledge, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(knowledge.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(knowledge.Text);
        return repository.UpsertAsync(knowledge, cancellationToken);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);
}
