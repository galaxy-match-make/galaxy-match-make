using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using Npgsql;

namespace galaxy_match_make.Repositories;


public class GenericRepository<T>: IGenericRepository<T> where T : class
{
    private readonly string _tableName;
    private readonly string _primaryKeyName = "Id";
    private readonly IConfiguration _configuration;
    
    public GenericRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        
        Type entityType = typeof(T);

        _tableName = entityType.GetCustomAttribute<TableAttribute>()?.Name
            ?? throw new NullReferenceException($"{entityType.Name} don't have a TableAttribute");
    }
    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }
    
    private string GetColumnNameFromProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        var pattern = @"(?<=[a-z0-9])(?=[A-Z])";
        return Regex.Replace(propertyName, pattern, "_").ToLowerInvariant();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        await using NpgsqlConnection connection = GetConnection();
        
        return await connection.QueryAsync<T>($"SELECT * FROM {_tableName}");
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        await using NpgsqlConnection connection = GetConnection();
        
        return await connection.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {_tableName} WHERE {_primaryKeyName} = @Id",
            new { Id = id });
    }

    public async Task<IEnumerable<T>> GetByColumnValueAsync(string columnName, object columnValue)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            return [];
        }
    
        await using NpgsqlConnection connection = GetConnection();
        
        columnValue = columnValue.GetType() == typeof(string) 
            ? $"'{columnValue}'"
            : columnValue;
        
        return await connection.QueryAsync<T>($"SELECT * FROM {_tableName} WHERE {columnName} = @ColumnValue",
            new { ColumnValue = columnValue });
    }

    public async Task<int> CreateAsync(T entity)
    {
        await using NpgsqlConnection connection = GetConnection();
        
        List<PropertyInfo> properties = typeof(T).GetProperties()
            .Where(p => p.Name != _primaryKeyName)
            .ToList();
        
        string columns = string.Join(", ", properties.Select(p => GetColumnNameFromProperty(p.Name)));
        string parameters = string.Join(", ", properties.Select(p => 
            p.PropertyType == typeof(string)
                ? $"'@{p.GetValue(entity)}'"
                : $"@{p.GetValue(entity)}"));
        
        string sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING {_primaryKeyName};";
        
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        await using NpgsqlConnection connection = GetConnection();
        
        List<PropertyInfo> properties = typeof(T).GetProperties()
            .Where(p => p.Name != _primaryKeyName)
            .ToList();

        string setClauses = string.Join(", ", properties.Select(p =>
        {
            string columnName = GetColumnNameFromProperty(p.Name);
            string columnValue = p.PropertyType == typeof(string)
                ? $"'@{p.GetValue(entity)}'"
                : $"@{p.GetValue(entity)}";

            return $"{columnName} = {columnValue}";
        }));
            

        PropertyInfo primaryKeyPropertyInfo = typeof(T).GetProperty(_primaryKeyName) ?? 
            throw new ArgumentException($"Unable to update entity  '{typeof(T).Name}' because it doesn't have a primary key property named '{_primaryKeyName}'");
        
        object primaryKeyValue = primaryKeyPropertyInfo.GetValue(entity) ?? 
            throw new ArgumentException($"Unable to update entity '{typeof(T).Name}' because primary key is null");

        var sql = $"UPDATE {_tableName} SET {setClauses} WHERE {_primaryKeyName} = @{primaryKeyValue}";
        
        return await connection.ExecuteAsync(sql, entity) > 0;
    }
}