using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public interface IReactionRepository
{
    Task<IEnumerable<ReactionDto>> GetAllReactionsAsync();
    Task<ReactionDto?> GetReactionByIdAsync(int id);
    Task AddReactionAsync(ReactionDto reaction);
    Task UpdateReactionAsync(ReactionDto reaction);
    Task DeleteReactionAsync(int id);
    Task<IEnumerable<ReactionDto>> GetReactionsByReactorAsync(Guid userId);
    Task<ReactionDto?> GetReactionByReactorAndTargetAsync(Guid reactorId, Guid targetId);
}