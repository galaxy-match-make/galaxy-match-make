using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories
{
    public interface IInterestRepository
    {
        Task<IEnumerable<InterestDto>> GetAllInterestsAsync();
        Task<InterestDto> GetInterestByIdAsync(int id);
    }
}