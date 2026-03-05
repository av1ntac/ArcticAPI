using ArcticApi.Logic.Abstractions;
using ArcticApi.Model;

namespace ArcticApi.Logic.Services;

public sealed class KnowledgeService(IKnowledgeRepository repository)
{
    private const string AuditUserId = "11111111-1111-1111-1111-111111111111";

    public Task<IReadOnlyCollection<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<Knowledge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, cancellationToken);

    public async Task<Knowledge> UpsertAsync(Knowledge knowledge, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(knowledge.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(knowledge.Text);

        var now = DateTime.UtcNow;
        var existing = await repository.GetByIdAsync(knowledge.Id, cancellationToken);

        knowledge.CreatedBy = existing?.CreatedBy ?? AuditUserId;
        knowledge.CreatedAt = existing?.CreatedAt ?? now;
        knowledge.ModifiedBy = AuditUserId;
        knowledge.ModifiedAt = now;

        return await repository.UpsertAsync(knowledge, cancellationToken);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);
}
