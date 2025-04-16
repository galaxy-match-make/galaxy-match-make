using galaxy_match_make.Models;

namespace galaxy_match_make.Services;

public interface IProfilePreferencesService : IGenericService<ProfilePreferencesDto>
{
    Task<IEnumerable<ProfilePreferencesDto>> GetProfilePreferencesForProfileId(int profileId);
}