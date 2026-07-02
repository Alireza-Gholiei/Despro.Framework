using Despro.Framework.Infrastructure.InfrastructureModels.DbModels.Mongo;

namespace Despro.Framework.Infrastructure.InfrastructureIServices;

public interface ILogService
{
    Task InsertManyAsync(IEnumerable<LogEntity> logEntries);
    void InsertMany(IEnumerable<LogEntity> logEntries);
}