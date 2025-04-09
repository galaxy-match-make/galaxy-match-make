using galaxy_match_make.Models;
using galaxy_match_make.Repositories;

namespace galaxy_match_make.Services;

public class PlanetService : IPlanetService
{
    private readonly IPlanetRepository _planetRepository;
    public PlanetService(IPlanetRepository planetRepository) => _planetRepository = planetRepository;

    public async Task<IEnumerable<PlanetDto>> GetAllPlanetsAsync()
    {
        var planets = await _planetRepository.GetAllPlanetsAsync();
        return planets;
    }

}
