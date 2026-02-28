namespace ArcticApi.Api;

public sealed class DynamoDbOptions
{
    public const string SectionName = "DynamoDb";

    public string ServiceUrl { get; set; } = "http://localhost:8000";
    public string Region { get; set; } = "eu-central-1";
}