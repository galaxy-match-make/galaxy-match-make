using galaxy_match_make.Models;

namespace galaxy_match_make.Services;

public interface IProfileService : IGenericService<ProfileDto>
{
    Task<IEnumerable<ProfileDto>> GetPreferredProfiles(int currentProfileId);
}