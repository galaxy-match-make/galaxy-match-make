using galaxy_match_make.Models;

namespace galaxy_match_make.Services;

public interface IPlanetService
{
    Task<IEnumerable<PlanetDto>> GetAllPlanetsAsync();
}
