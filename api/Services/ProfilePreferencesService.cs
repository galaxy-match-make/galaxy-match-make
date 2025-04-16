using galaxy_match_make.Models;
using galaxy_match_make.Repositories;

namespace galaxy_match_make.Services;

public class ProfilePreferencesService(
    IGenericRepository<ProfilePreferencesDto> profilePreferencesRepository) 
    : GenericService<ProfilePreferencesDto>(profilePreferencesRepository), IProfilePreferencesService
{
    public async Task<IEnumerable<ProfilePreferencesDto>> GetProfilePreferencesForProfileId(int profileId)
    {
        return await profilePreferencesRepository
                .GetByColumnValueAsync("profile_id", profileId);
    }
}