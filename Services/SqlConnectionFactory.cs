using DbMigrator.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DbMigrator.Services;

public class SqlConnectionFactory : IDbConnectionFactory
{
    public IDbConnection CreateConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.");
        }

        return new SqlConnection(connectionString);
    }
}