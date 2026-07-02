using Despro.Framework.Infrastructure.InfrastructureIServices;
using Despro.Framework.Infrastructure.InfrastructureModels.DbModels;

namespace Despro.Framework.Infrastructure.InfrastructureServices;

public class NullLoggingContext : ILoggingContext
{
    public void AddLog(string entityName, OperationLogType operation, object? data)
    {

    }

    public Task FlushLogsAsync()
    {
        return Task.CompletedTask;
    }

    public void FlushLogs()
    {

    }
}