using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ArcticApi.Api;
using ArcticApi.Logic.Abstractions;
using ArcticApi.Logic.Services;
using ArcticApi.Model;
using ArcticApi.Orm.Repositories;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DynamoDbOptions>(builder.Configuration.GetSection(DynamoDbOptions.SectionName));
builder.Services.AddSingleton<IAmazonDynamoDB>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<DynamoDbOptions>>().Value;
    var config = new AmazonDynamoDBConfig
    {
        ServiceURL = options.ServiceUrl,
        AuthenticationRegion = options.Region,
        UseHttp = options.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
    };
    var credentials = new Amazon.Runtime.BasicAWSCredentials("local", "local");
    return new AmazonDynamoDBClient(credentials, config);
});
builder.Services.AddScoped<IKnowledgeRepository, DynamoDbKnowledgeRepository>();
builder.Services.AddScoped<IPlayerRepository, DynamoDbPlayerRepository>();
builder.Services.AddScoped<KnowledgeService>();
builder.Services.AddScoped<PlayerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health/dynamodb", async (IAmazonDynamoDB dynamoDb, CancellationToken cancellationToken) =>
{
    try
    {
        await dynamoDb.ListTablesAsync(new ListTablesRequest { Limit = 1 }, cancellationToken);
        return Results.Ok(new { status = "ok" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"DynamoDB connection failed: {ex.Message}");
    }
});

var players = app.MapGroup("/players");
players.MapGet("/", async (PlayerService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetAllAsync(cancellationToken)));

players.MapGet("/{id}", async (string id, PlayerService service, CancellationToken cancellationToken) =>
{
    var player = await service.GetByIdAsync(id, cancellationToken);
    return player is null ? Results.NotFound() : Results.Ok(player);
});

players.MapPut("/{id}", async (string id, Player request, PlayerService service, CancellationToken cancellationToken) =>
{
    request.Id = id;
    var saved = await service.UpsertAsync(request, cancellationToken);
    return Results.Ok(saved);
});

players.MapDelete("/{id}", async (string id, PlayerService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

var knowledge = app.MapGroup("/knowledge");
knowledge.MapGet("/", async (KnowledgeService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetAllAsync(cancellationToken)));

knowledge.MapGet("/{id}", async (string id, KnowledgeService service, CancellationToken cancellationToken) =>
{
    var item = await service.GetByIdAsync(id, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

knowledge.MapPut("/{id}", async (string id, Knowledge request, KnowledgeService service, CancellationToken cancellationToken) =>
{
    request.Id = id;
    var saved = await service.UpsertAsync(request, cancellationToken);
    return Results.Ok(saved);
});

knowledge.MapDelete("/{id}", async (string id, KnowledgeService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();
