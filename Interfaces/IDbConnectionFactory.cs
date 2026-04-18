using System.Data;

namespace DbMigrator.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection(string connectionString);}