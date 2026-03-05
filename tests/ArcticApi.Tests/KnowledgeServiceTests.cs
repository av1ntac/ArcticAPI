using ArcticApi.Logic.Abstractions;
using ArcticApi.Logic.Services;
using ArcticApi.Model;
using Xunit;

namespace ArcticApi.Tests;

public class KnowledgeServiceTests
{
    private const string AuditUserId = "11111111-1111-1111-1111-111111111111";

    [Fact]
    public async Task UpsertAsync_StoresKnowledgeInRepository()
    {
        var repository = new InMemoryKnowledgeRepository();
        var service = new KnowledgeService(repository);

        var knowledge = new Knowledge { Title = "Ritual", Text = "Use at dusk." };
        await service.UpsertAsync(knowledge);

        var stored = await service.GetByIdAsync(knowledge.Id);
        Assert.NotNull(stored);
        Assert.Equal("Ritual", stored.Title);
        Assert.Equal(AuditUserId, stored.CreatedBy);
        Assert.Equal(AuditUserId, stored.ModifiedBy);
        Assert.NotEqual(default, stored.CreatedAt);
        Assert.NotEqual(default, stored.ModifiedAt);
    }

    [Fact]
    public async Task UpsertAsync_OnExistingKnowledge_PreservesCreatedAuditAndRefreshesModifiedAudit()
    {
        var repository = new InMemoryKnowledgeRepository();
        var service = new KnowledgeService(repository);

        var initial = new Knowledge { Title = "Ritual", Text = "Use at dusk." };
        await service.UpsertAsync(initial);

        var firstSaved = await service.GetByIdAsync(initial.Id);
        Assert.NotNull(firstSaved);

        await Task.Delay(5);

        initial.Text = "Use at dawn.";
        await service.UpsertAsync(initial);

        var secondSaved = await service.GetByIdAsync(initial.Id);
        Assert.NotNull(secondSaved);
        Assert.Equal(firstSaved.CreatedBy, secondSaved.CreatedBy);
        Assert.Equal(firstSaved.CreatedAt, secondSaved.CreatedAt);
        Assert.Equal(AuditUserId, secondSaved.ModifiedBy);
        Assert.True(secondSaved.ModifiedAt >= firstSaved.ModifiedAt);
    }

    [Fact]
    public async Task UpsertAsync_ThrowsWhenTitleMissing()
    {
        var repository = new InMemoryKnowledgeRepository();
        var service = new KnowledgeService(repository);

        var knowledge = new Knowledge { Title = "", Text = "Lore" };
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpsertAsync(knowledge));
    }

    private sealed class InMemoryKnowledgeRepository : IKnowledgeRepository
    {
        private readonly Dictionary<string, Knowledge> _items = [];

        public Task<IReadOnlyCollection<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyCollection<Knowledge>)_items.Values.ToList());

        public Task<Knowledge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(id));

        public Task<Knowledge> UpsertAsync(Knowledge knowledge, CancellationToken cancellationToken = default)
        {
            _items[knowledge.Id] = knowledge;
            return Task.FromResult(knowledge);
        }

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Remove(id));
    }
}
