using galaxy_match_make.Models;
using galaxy_match_make.Repositories;

namespace galaxy_match_make.Services;

public class ProfileService(
    IGenericRepository<ProfileDto> profileRepository, 
    IGenericRepository<ProfileAttributesDto> profileAttributesRepository, 
    IGenericRepository<ProfilePreferencesDto> profilePreferencesRepository) 
    : GenericService<ProfileDto>(profileRepository), IProfileService
{
    public async Task<IEnumerable<ProfileDto>> GetPreferredProfiles(int currentProfileId)
    {
        List<ProfilePreferencesDto> currentUserPreferences = (await profilePreferencesRepository
            .GetByColumnValueAsync("profile_id", currentProfileId))
                .ToList();

        if (!currentUserPreferences.Any())
        {
            return [];
        }

        List<ProfileDto> allOtherProfiles = (await profileRepository.GetAllAsync())
                .Where(profile => profile.Id != currentProfileId)
                .ToList();
        
        List<ProfileDto> preferredProfiles = new List<ProfileDto>();

        foreach (ProfileDto otherProfile in allOtherProfiles)
        {
            var otherProfileAttributes = await profileAttributesRepository.GetByColumnValueAsync("profile_id", otherProfile.Id);

            bool isPreferredProfile = true;
            
            foreach (ProfilePreferencesDto profilePreference in currentUserPreferences)
            {
                if (!otherProfileAttributes.Any(profileAttribute => profileAttribute.CharacteristicId == profilePreference.CharacteristicId))
                {
                    isPreferredProfile = false;
                    break;
                }
            }

            if (isPreferredProfile)
            {
                preferredProfiles.Add(otherProfile);
            }
        }

        return preferredProfiles;
    }

}