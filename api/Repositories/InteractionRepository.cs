using System.Data;
using Dapper;
using galaxy_match_make.Models;
using Npgsql;

namespace galaxy_match_make.Repositories;

public class InteractionRepository : IInteractionRepository
{
    private readonly string _connectionString;

    public InteractionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<ContactDto>> GetReactions(Guid userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
        SELECT 
            p.user_id,
            p.display_name,
            p.avatar_url
        FROM reactions r1
        JOIN reactions r2 
            ON r1.target_id = r2.reactor_id 
           AND r1.reactor_id = r2.target_id
           AND r2.is_positive = true
        JOIN profiles p ON r1.target_id = p.user_id
        WHERE r1.reactor_id = @UserId
          AND r1.is_positive = true";

        var results = await connection.QueryAsync<ContactDto>(query, new { UserId = userId });
        return results.ToList();
    }

    public async Task<List<ContactDto>> GetSentRequests(Guid userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
        SELECT 
            p.user_id,
            p.display_name,
            p.avatar_url
        FROM reactions r1
        JOIN profiles p ON r1.target_id = p.user_id
        WHERE r1.reactor_id = @UserId
          AND r1.is_positive = true
          AND NOT EXISTS (
              SELECT 1 
              FROM reactions r2 
              WHERE r2.reactor_id = r1.target_id
                AND r2.target_id = @UserId
          )
        ";

        var results = await connection.QueryAsync<ContactDto>(query, new { UserId = userId });
        return results.ToList();
    }

    public async Task<List<ContactDto>> GetReceivedRequests(Guid userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
        SELECT 
            p.user_id,
            p.display_name,
            p.avatar_url
        FROM reactions r1
        JOIN profiles p ON r1.reactor_id = p.user_id
        WHERE r1.target_id = @UserId
          AND r1.is_positive = true
          AND NOT EXISTS (
              SELECT 1 
              FROM reactions r2 
              WHERE r2.reactor_id = @UserId
                AND r2.target_id = r1.reactor_id
          )";


        var results = await connection.QueryAsync<ContactDto>(query, new { UserId = userId });
        return results.ToList();
    }

    public async Task ReactToRequest(Guid reactorId, Guid targetId, bool isPositive = true)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
        INSERT INTO reactions (reactor_id, target_id, is_positive)
        VALUES (@ReactorId, @TargetId, @IsPositive)";

        await connection.ExecuteAsync(query, new 
        {
            ReactorId = reactorId,
            TargetId = targetId,
            IsPositive = isPositive
        });
    }
    
    public async Task CancelRequest(Guid reactorId, Guid targetId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await connection.ExecuteAsync(
            @"DELETE FROM reactions 
          WHERE reactor_id = @ReactorId 
            AND target_id = @TargetId
            AND is_positive = true",
            new { ReactorId = reactorId, TargetId = targetId });
        
    }

    public async Task<List<ContactDto>> GetRequestsDeclinedByUser(Guid userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
        SELECT 
            p.user_id,
            p.display_name,
            p.avatar_url
        FROM reactions r1
        JOIN profiles p ON r1.target_id = p.user_id
        WHERE r1.reactor_id = @UserId
          AND r1.is_positive = false
          ";

        var results = await connection.QueryAsync<ContactDto>(query, new { UserId = userId });
        return results.ToList();
    }
}

 