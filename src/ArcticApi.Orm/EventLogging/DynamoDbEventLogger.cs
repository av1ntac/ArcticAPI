using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;

namespace ArcticApi.Orm.EventLogging;

public sealed class DynamoDbEventLogger(IAmazonDynamoDB dynamoDb)
{
    private const string TableName = "event_log";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        object? beforeState,
        object? afterState,
        CancellationToken cancellationToken = default)
    {
        var table = Table.LoadTable(dynamoDb, TableName);
        var now = DateTime.UtcNow;

        var doc = new Document
        {
            ["Id"] = Guid.NewGuid().ToString("N"),
            ["Timestamp"] = now.ToString("O"),
            ["Action"] = action,
            ["EntityType"] = entityType,
            ["EntityId"] = entityId ?? string.Empty,
            ["BeforeState"] = Serialize(beforeState),
            ["AfterState"] = Serialize(afterState)
        };

        return table.PutItemAsync(doc, cancellationToken);
    }

    private static string Serialize(object? value)
        => JsonSerializer.Serialize(value, JsonOptions);
}
