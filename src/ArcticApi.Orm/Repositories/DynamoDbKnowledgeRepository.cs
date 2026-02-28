using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using ArcticApi.Logic.Abstractions;
using ArcticApi.Model;

namespace ArcticApi.Orm.Repositories;

public sealed class DynamoDbKnowledgeRepository(IAmazonDynamoDB dynamoDb) : IKnowledgeRepository
{
    private const string TableName = "knowledge";

    public async Task<IReadOnlyCollection<Knowledge>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var search = table.Scan(new ScanOperationConfig());

        var output = new List<Knowledge>();
        do
        {
            var page = await search.GetNextSetAsync(cancellationToken);
            output.AddRange(page.Select(MapKnowledge));
        } while (!search.IsDone);

        return output;
    }

    public async Task<Knowledge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var doc = await table.GetItemAsync(id, cancellationToken);
        return doc is null ? null : MapKnowledge(doc);
    }

    public async Task<Knowledge> UpsertAsync(Knowledge knowledge, CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var doc = new Document
        {
            ["Id"] = knowledge.Id,
            ["Title"] = knowledge.Title,
            ["Text"] = knowledge.Text
        };

        await table.PutItemAsync(doc, cancellationToken);
        return knowledge;
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

    private static Knowledge MapKnowledge(Document doc) => new()
    {
        Id = doc[nameof(Knowledge.Id)].AsString(),
        Title = doc[nameof(Knowledge.Title)].AsString(),
        Text = doc[nameof(Knowledge.Text)].AsString()
    };
}
