using galaxy_match_make.Models;
using galaxy_match_make.Repositories;

namespace galaxy_match_make.Services;

public class ProfileAttributesService(
    IGenericRepository<ProfileAttributesDto> profileAttributesRepository) 
    : GenericService<ProfileAttributesDto>(profileAttributesRepository), IProfileAttributesService
{
    public async Task<IEnumerable<ProfileAttributesDto>> GetProfilePreferencesForProfileId(int profileId)
    {
        return await profileAttributesRepository
                .GetByColumnValueAsync("profile_id", profileId);
    }
}