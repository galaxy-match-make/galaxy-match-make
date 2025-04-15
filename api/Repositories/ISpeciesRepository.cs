using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public interface ISpeciesRepository
{
    Task<IEnumerable<SpeciesDto>> GetAllSpeciesAsync();
    Task<SpeciesDto?> GetSpeciesByIdAsync(int id);
}