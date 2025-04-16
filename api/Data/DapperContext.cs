using System.Data;
using Npgsql;

namespace galaxy_match_make.Data;
public class DapperContext
{
    private readonly string _connectionString;
    private readonly IConfiguration _config;

    // Constructor for configuration-based initialization
    public DapperContext(IConfiguration config)
    {
        _config = config;
        _connectionString = _config.GetConnectionString("DefaultConnection");
    }

    // Constructor for direct connection string initialization
    public DapperContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
}
