using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public class ReactionsRepository : IReactionRepository
{
    private readonly DapperContext _context;

    public ReactionsRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReactionDto>> GetAllReactionsAsync()
    {
        const string query = "SELECT * FROM reactions";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ReactionDto>(query);
    }

    public async Task<ReactionDto?> GetReactionByIdAsync(int id)
    {
        const string query = "SELECT * FROM reactions WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ReactionDto>(query, new { Id = id });
    }

    public async Task AddReactionAsync(ReactionDto reaction)
    {
        const string query = "INSERT INTO reactions (reactor_id, target_id, is_positive) VALUES (@ReactorId, @TargetId, @IsPositive)";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, reaction);
    }

    public async Task UpdateReactionAsync(ReactionDto reaction)
    {
        const string query = "UPDATE reactions SET reactor_id = @ReactorId, target_id = @TargetId, is_positive = @IsPositive WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, reaction);
    }

    public async Task DeleteReactionAsync(int id)
    {
        const string query = "DELETE FROM reactions WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, new { Id = id });
    }

    public async Task<IEnumerable<ReactionDto>> GetReactionsByReactorAsync(Guid userId)
    {
        const string query = "SELECT * FROM reactions WHERE reactor_id = @ReactorId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ReactionDto>(query, new { ReactorId = userId });
    }

    public async Task<ReactionDto?> GetReactionByReactorAndTargetAsync(Guid reactorId, Guid targetId)
    {
        const string query = "SELECT * FROM reactions WHERE reactor_id = @ReactorId AND target_id = @TargetId";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ReactionDto>(query, new { ReactorId = reactorId, TargetId = targetId });
    }
}