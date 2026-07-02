using Despro.Framework.Infrastructure.InfrastructureIServices;
using Despro.Framework.Infrastructure.InfrastructureModels.DbModels.Mongo;
using MongoDB.Driver;

namespace Despro.Framework.Infrastructure.InfrastructureServices;

public class MongoLogService : ILogService
{
    private readonly IMongoCollection<LogEntity> _logs;
    public MongoLogService(IMongoDatabase database)
    {
        _logs = database.GetCollection<LogEntity>("LogEntries");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexKeys = Builders<LogEntity>.IndexKeys
            .Ascending(x => x.EntityName)
            .Ascending(x => x.ActionDate);

        _logs.Indexes.CreateOne(new CreateIndexModel<LogEntity>(indexKeys));

        var userIdIndex = Builders<LogEntity>.IndexKeys.Ascending(x => x.UserId);
        _logs.Indexes.CreateOne(new CreateIndexModel<LogEntity>(userIdIndex));

        var logTypeIndex = Builders<LogEntity>.IndexKeys.Ascending(x => x.LogType);
        _logs.Indexes.CreateOne(new CreateIndexModel<LogEntity>(logTypeIndex));
    }

    public async Task InsertManyAsync(IEnumerable<LogEntity> logEntries)
    {
        try
        {
            await _logs.InsertManyAsync(logEntries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Batch logging failed: {ex.Message}");
        }
    }

    public void InsertMany(IEnumerable<LogEntity> logEntries)
    {
        try
        {
            _logs.InsertMany(logEntries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Batch logging failed: {ex.Message}");
        }
    }
}