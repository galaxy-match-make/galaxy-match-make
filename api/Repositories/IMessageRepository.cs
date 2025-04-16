namespace galaxy_match_make.Repositories;

using galaxy_match_make.Models;

public interface IMessageRepository
{
    Task<IEnumerable<MessageDto>> GetAllMessagesAsync();
    Task<MessageDto?> GetMessageByIdAsync(int id);
    Task AddMessageAsync(MessageDto message);
    Task UpdateMessageAsync(MessageDto message);
    Task DeleteMessageAsync(int id);

    Task<IEnumerable<MessageDto>> GetMessagesBetweenTwoUsers(Guid senderId, Guid receiverId);
}
