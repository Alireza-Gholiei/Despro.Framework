using Despro.Framework.Infrastructure.InfrastructureIServices;
using Despro.Framework.Infrastructure.InfrastructureModels.DbModels.Mongo;

namespace Despro.Framework.Infrastructure.InfrastructureServices;

public class NullLogService : ILogService
{
    public Task InsertManyAsync(IEnumerable<LogEntity> logEntries)
    {
        return Task.CompletedTask;
    }

    public void InsertMany(IEnumerable<LogEntity> logEntries)
    {

    }
}