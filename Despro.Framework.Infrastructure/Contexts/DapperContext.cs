namespace Despro.Framework.Infrastructure.Contexts;

public class DapperContext(string connectionString)
{
    //public IDbConnection CreateConnection() => new SqlConnection(connectionString);

    public string test => "[test].Item";
}