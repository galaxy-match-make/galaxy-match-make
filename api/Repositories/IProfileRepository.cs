using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories
{
    public interface IProfileRepository
    {
        Task<IEnumerable<ProfileDto>> GetAllProfiles();
        Task<ProfileDto> GetProfileById(Guid id);
        Task<ProfileDto> UpdateProfile(Guid id, UpdateProfileDto profile);
        Task<ProfileDto> CreateProfile(Guid id, CreateProfileDto profile);

        Task<IEnumerable<ProfileDto>> GetPendingMatchesByUserId(Guid id);
        Task<IEnumerable<MatchedProfileDto>> GetUserMatchedProfiles(Guid id);
        Task<IEnumerable<LikersDto>> GetUserLikersProfiles(Guid id);
    }
}
