using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.InfrastructureIServices;
using Despro.Framework.Infrastructure.InfrastructureModels.DbModels;
using Despro.Framework.Infrastructure.InfrastructureModels.DbModels.Mongo;
using MongoDB.Bson;
using System.Collections;
using System.Reflection;

namespace Despro.Framework.Infrastructure.InfrastructureServices;

public class LoggingContext(IAuthService authService, ILogService logService) : ILoggingContext
{
    private readonly List<(string EntityName, OperationLogType Operation, object Data, long UserId)> _pendingLogs = [];
    private const int BatchSize = 100;

    public void AddLog(string entityName, OperationLogType operation, object? data)
    {
        _pendingLogs.Add((entityName, operation, data, authService.GetUserId())!);
    }

    public async Task FlushLogsAsync()
    {
        if (_pendingLogs != null && !_pendingLogs.Any())
            return;

        foreach (var batch in _pendingLogs.Chunk(BatchSize))
        {
            var logEntries = batch.Select(log => new LogEntity
            {
                EntityName = log.EntityName,
                LogType = log.Operation,
                UserId = log.UserId,
                ActionDate = DateTime.UtcNow,
                EntityData = ToBsonDocument(log.Data)
            }).ToList();

            await logService.InsertManyAsync(logEntries);
        }

        _pendingLogs.Clear();
    }

    public void FlushLogs()
    {
        if (!_pendingLogs.Any()) return;

        foreach (var batch in _pendingLogs.Chunk(BatchSize))
        {
            var logEntries = batch.Select(log => new LogEntity
            {
                EntityName = log.EntityName,
                LogType = log.Operation,
                UserId = log.UserId,
                ActionDate = DateTime.UtcNow,
                EntityData = ToBsonDocument(log.Data)
            }).ToList();

            logService.InsertManyAsync(logEntries).GetAwaiter().GetResult();
        }

        _pendingLogs.Clear();
    }

    private readonly HashSet<object> _processed = [];

    private BsonValue ToBsonDocument(object? data)
    {
        switch (data)
        {
            case null:
                return BsonNull.Value;
            case Guid g:
                return new BsonString(g.ToString());
            case Enum e:
                return new BsonString(e.ToString());
        }

        var type = data.GetType();

        if (type.IsPrimitive || data is string || data is decimal || data is DateTime)
            return BsonValue.Create(data);

        if (_processed.Contains(data))
            return BsonNull.Value;

        _processed.Add(data);

        if (data is IEnumerable collection and not string)
        {
            var array = new BsonArray();
            foreach (var item in collection)
                array.Add(ToBsonDocument(item));
            return array;
        }

        var document = new BsonDocument();

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (var prop in properties)
        {
            if (prop.Name == "DomainEvents")
                continue;

            var value = prop.GetValue(data);
            if (value == null)
                continue;

            if (value is IEnumerable col and not string)
            {
                var array = new BsonArray();

                foreach (var item in col)
                {
                    if (item != null && !_processed.Contains(item))
                        array.Add(ToBsonDocument(item));
                }

                document.Add(prop.Name, array);
            }
            else if (!_processed.Contains(value))
            {
                document.Add(prop.Name, ToBsonDocument(value));
            }
        }

        return document;
    }

    //private BsonValue ToBsonDocument(object? data)
    //{
    //    if (data == null)
    //        return BsonNull.Value;

    //    if (data is Guid g)
    //        return new BsonString(g.ToString());

    //    var type = data.GetType();

    //    if (type.IsPrimitive || type.IsEnum || data is string || data is decimal || data is DateTime || data is Guid)
    //        return BsonValue.Create(data);

    //    if (data is IEnumerable collection and not string)
    //    {
    //        var array = new BsonArray();
    //        foreach (var item in collection)
    //        {
    //            if (item == null)
    //            {
    //                array.Add(BsonNull.Value);
    //            }
    //            else
    //            {
    //                var itemType = item.GetType();
    //                if (itemType.IsPrimitive || item is string || item is decimal || item is DateTime || item is Guid)
    //                    array.Add(BsonValue.Create(item));
    //                else
    //                    array.Add(ToBsonDocument(item));
    //            }
    //        }
    //        return array;
    //    }

    //    var document = new BsonDocument();
    //    var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
    //        .Where(p => p.Name != "DomainEvents");

    //    foreach (var prop in properties)
    //    {
    //        var value = prop.GetValue(data);
    //        if (value != null)
    //            document.Add(prop.Name, ToBsonDocument(value));
    //    }

    //    return document;
    //}

    //private BsonDocument ToBsonDocument(object data)
    //{
    //    var document = new BsonDocument();

    //    if (data == null)
    //        return document;

    //    if (data is IEnumerable collection and not string)
    //    {
    //        var array = new BsonArray();
    //        foreach (var item in collection)
    //        {
    //            array.Add(ToBsonDocument(item));
    //        }
    //        return new BsonDocument("Items", array);
    //    }

    //    var type = data.GetType();
    //    var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
    //        .Where(p => p.Name != "DomainEvents");

    //    foreach (var prop in properties)
    //    {
    //        var value = prop.GetValue(data);
    //        if (value == null) continue;

    //        if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
    //        {
    //            document.Add(prop.Name, ToBsonDocument(value));
    //        }
    //        else
    //        {
    //            document.Add(prop.Name, BsonValue.Create(value));
    //        }
    //    }

    //    return document;
    //}
}