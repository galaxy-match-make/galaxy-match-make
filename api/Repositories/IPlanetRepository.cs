using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public interface IPlanetRepository
{
    Task<IEnumerable<PlanetDto>> GetAllPlanetsAsync();
    Task<PlanetDto> GetPlanetByIdAsync(int id);

}