using galaxy_match_make.Models;

namespace galaxy_match_make.Services;

public interface IProfileAttributesService : IGenericService<ProfileAttributesDto>
{
    Task<IEnumerable<ProfileAttributesDto>> GetProfilePreferencesForProfileId(int profileId);
}