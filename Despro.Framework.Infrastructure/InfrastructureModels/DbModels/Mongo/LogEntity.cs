using MongoDB.Bson;

namespace Despro.Framework.Infrastructure.InfrastructureModels.DbModels.Mongo;

public class LogEntity
{
    public ObjectId Id { get; set; }
    public string EntityName { get; set; }
    public OperationLogType LogType { get; set; }
    public long UserId { get; set; }
    public DateTime ActionDate { get; set; }
    public BsonValue EntityData { get; set; }
    //public BsonDocument EntityData { get; set; }
}