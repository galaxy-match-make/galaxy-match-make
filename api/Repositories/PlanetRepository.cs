using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public class PlanetRepository : IPlanetRepository
{
    private readonly DapperContext _context;
    public PlanetRepository(DapperContext context) => _context = context;

    public async Task<IEnumerable<PlanetDto>> GetAllPlanetsAsync()
    {
        var query = "SELECT id, planet_name FROM planets";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<PlanetDto>(query);
    }

    public async Task<PlanetDto> GetPlanetByIdAsync(int id)
    {
        var query = @"SELECT id, planet_name FROM planets WHERE id=@id";

        var connection = _context.CreateConnection();

        var planet = await connection.QuerySingleOrDefaultAsync<PlanetDto>(query, new { id });

        return planet!;
    }
}
