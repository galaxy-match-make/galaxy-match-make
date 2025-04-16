using System.Data;
using Npgsql;

namespace galaxy_match_make.Data;

public class AppDbContext : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection _connection;
    private IDbTransaction _transaction;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
        _connection = new NpgsqlConnection(_connectionString);
        _connection.Open();
    }

    public IDbConnection Connection => _connection;
    public IDbTransaction Transaction => _transaction;

    public void BeginTransaction()
    {
        _transaction = _connection.BeginTransaction();
    }

    public void CommitTransaction()
    {
        _transaction?.Commit();
    }

    public void RollbackTransaction()
    {
        _transaction?.Rollback();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}