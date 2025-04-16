using System.Data;
using System.Threading.Tasks;
using galaxy_match_make.Services;
using Npgsql;
using System;

namespace galaxy_match_make.Data;
public class DapperContext
{
    private readonly IConfiguration _config;
    private readonly IAppConfigurationProvider _appConfigProvider;
    private string _connectionString;
    private readonly string _environment;
    private readonly string _secretPrefix;
    
    public DapperContext(IConfiguration config, IAppConfigurationProvider appConfigProvider = null) 
    {
        _config = config;
        _appConfigProvider = appConfigProvider;
        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        _secretPrefix = Environment.GetEnvironmentVariable("AWS_SECRET_PREFIX") ?? "";
        
        // For immediate use, we'll initialize with the configuration
        _connectionString = _config.GetConnectionString("DefaultConnection");
        
        // Asynchronously try to get the connection string from AWS Secrets Manager
        if (_appConfigProvider != null && _environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            Task.Run(async () =>
            {
                try
                {
                    // Get connection string from AWS Secrets Manager with appropriate suffix
                    _connectionString = await _appConfigProvider.GetSecretAsync("ConnectionStrings/DefaultConnection");
                }
                catch
                {
                    // If there's an error, we'll fall back to the appsettings.json connection string
                }
            }).Wait();
        }
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
}
