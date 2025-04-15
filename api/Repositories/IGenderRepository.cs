using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public interface IGenderRepository
{
    Task<IEnumerable<GenderDto>> GetAllGendersAsync();
    Task<GenderDto?> GetGenderByIdAsync(int id);
}