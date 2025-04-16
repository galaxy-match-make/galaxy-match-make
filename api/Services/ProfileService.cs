using galaxy_match_make.Models;
using galaxy_match_make.Repositories;

namespace galaxy_match_make.Services;

public class ProfileService(
    IGenericRepository<ProfileDto> profileRepository, 
    IProfileAttributesService profileAttributesService, 
    IProfilePreferencesService profilePreferencesService) 
    : GenericService<ProfileDto>(profileRepository), IProfileService
{
    public async Task<IEnumerable<ProfileDto>> GetPreferredProfiles(int currentProfileId)
    {
        List<int> currentUserPreferencesCharacteristicIds = (await profilePreferencesService
            .GetProfilePreferencesForProfileId(currentProfileId))
            .Select(preference => preference.CharacteristicId)
            .ToList();

        if (!currentUserPreferencesCharacteristicIds.Any())
        {
            return [];
        }

        List<ProfileDto> allOtherProfiles = (await profileRepository.GetAllAsync())
                .Where(profile => profile.Id != currentProfileId)
                .ToList();
        
        List<ProfileDto> preferredProfiles = new List<ProfileDto>();

        foreach (ProfileDto otherProfile in allOtherProfiles)
        {
            List<int> otherProfileAttributesCharacteristicIds = (await profileAttributesService
                    .GetProfilePreferencesForProfileId(otherProfile.Id))
                    .Select(attribute => attribute.CharacteristicId)
                    .ToList();

            bool isPreferredProfile = currentUserPreferencesCharacteristicIds
                .All(preferenceCharacteristicId =>
                    otherProfileAttributesCharacteristicIds.Contains(preferenceCharacteristicId));

            if (isPreferredProfile)
            {
                preferredProfiles.Add(otherProfile);
            }
        }

        return preferredProfiles;
    }
}