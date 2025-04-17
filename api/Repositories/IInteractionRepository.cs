using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories;

public interface IInteractionRepository
{
    Task<List<ContactDto>> GetReactions(Guid userId);
    Task<List<ContactDto>> GetSentRequests(Guid userId);
    Task<List<ContactDto>> GetReceivedRequests(Guid userId);
    Task ReactToRequest(Guid reactorId, Guid targetId, bool isPositive);
    Task CancelRequest(Guid reactorId, Guid targetId);
    Task<List<ContactDto>> GetRequestsDeclinedByUser(Guid userId);
}