using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using ArcticApi.Logic.Abstractions;
using ArcticApi.Model;

namespace ArcticApi.Orm.Repositories;

public sealed class DynamoDbPlayerRepository(IAmazonDynamoDB dynamoDb) : IPlayerRepository
{
    private const string TableName = "players";

    public async Task<IReadOnlyCollection<Player>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var search = table.Scan(new ScanOperationConfig());

        var output = new List<Player>();
        do
        {
            var page = await search.GetNextSetAsync(cancellationToken);
            output.AddRange(page.Select(MapPlayer));
        } while (!search.IsDone);

        return output;
    }

    public async Task<Player?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var doc = await table.GetItemAsync(id, cancellationToken);
        return doc is null ? null : MapPlayer(doc);
    }

    public async Task<Player> UpsertAsync(Player player, CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var knowledgeList = new DynamoDBList();
        foreach (var knowledge in player.Knowledge)
        {
            knowledgeList.Add(MapKnowledgeToEntry(knowledge));
        }

        var doc = new Document
        {
            ["Id"] = player.Id,
            ["Name"] = player.Name,
            ["Health"] = player.Health,
            ["Knowledge"] = knowledgeList
        };

        await table.PutItemAsync(doc, cancellationToken);
        return player;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var existing = await table.GetItemAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await table.DeleteItemAsync(id, cancellationToken);
        return true;
    }

    private static Player MapPlayer(Document doc)
    {
        var output = new Player
        {
            Id = doc[nameof(Player.Id)].AsString(),
            Name = doc[nameof(Player.Name)].AsString(),
            Health = doc[nameof(Player.Health)].AsInt()
        };

        if (doc.TryGetValue(nameof(Player.Knowledge), out var knowledgeEntries) && knowledgeEntries is DynamoDBList list)
        {
            output.Knowledge = list.Entries
                .OfType<Document>()
                .Select(MapKnowledge)
                .ToList();
        }

        return output;
    }

    private static Knowledge MapKnowledge(Document doc) => new()
    {
        Id = doc[nameof(Knowledge.Id)].AsString(),
        Title = doc[nameof(Knowledge.Title)].AsString(),
        Text = doc[nameof(Knowledge.Text)].AsString()
    };

    private static Document MapKnowledgeToEntry(Knowledge knowledge) => new()
    {
        [nameof(Knowledge.Id)] = knowledge.Id,
        [nameof(Knowledge.Title)] = knowledge.Title,
        [nameof(Knowledge.Text)] = knowledge.Text
    };
}
