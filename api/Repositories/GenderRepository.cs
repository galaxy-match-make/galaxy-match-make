using Dapper;
using galaxy_match_make.Models;
using System.Data;

namespace galaxy_match_make.Repositories;

public class GenderRepository : IGenderRepository
{
    private readonly IDbConnection _dbConnection;

    public GenderRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IEnumerable<GenderDto>> GetAllGendersAsync()
    {
        const string query = "SELECT id, gender as Gender FROM genders";
        return await _dbConnection.QueryAsync<GenderDto>(query);
    }

    public async Task<GenderDto?> GetGenderByIdAsync(int id)
    {
        const string query = "SELECT id, gender as Gender FROM genders WHERE id = @Id";
        return await _dbConnection.QuerySingleOrDefaultAsync<GenderDto>(query, new { Id = id });
    }
}