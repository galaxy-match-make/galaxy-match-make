using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;
using System.Data;

namespace galaxy_match_make.Repositories;

public class GenderRepository : IGenderRepository
{
    private readonly DapperContext _context;

    public GenderRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GenderDto>> GetAllGendersAsync()
    {
        const string query = "SELECT id, gender as Gender FROM genders";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<GenderDto>(query);
    }

    public async Task<GenderDto?> GetGenderByIdAsync(int id)
    {
        const string query = "SELECT id, gender as Gender FROM genders WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<GenderDto>(query, new { Id = id });
    }
}