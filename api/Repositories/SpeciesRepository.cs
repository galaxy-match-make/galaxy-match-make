using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;
using System.Data;

namespace galaxy_match_make.Repositories;

public class SpeciesRepository : ISpeciesRepository
{
    private readonly DapperContext _context;

    public SpeciesRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SpeciesDto>> GetAllSpeciesAsync()
    {
        const string query = "SELECT id, species_name as SpeciesName FROM species";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SpeciesDto>(query);
    }

    public async Task<SpeciesDto?> GetSpeciesByIdAsync(int id)
    {
        const string query = "SELECT id, species_name as SpeciesName FROM species WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SpeciesDto>(query, new { Id = id });
    }
}