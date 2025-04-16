using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;
using System.Data;

namespace galaxy_match_make.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly DapperContext _context;

    public MessageRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MessageDto>> GetAllMessagesAsync()
    {
        const string query = "SELECT * FROM messages";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<MessageDto>(query);
    }

    public async Task<MessageDto?> GetMessageByIdAsync(int id)
    {
        const string query = "SELECT * FROM messages WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<MessageDto>(query, new { Id = id });
    }

    public async Task AddMessageAsync(MessageDto message)
    {
        const string query = "INSERT INTO messages (message_content, sent_date, sender_id, recipient_id) VALUES (@MessageContent, @SentDate, @SenderId, @RecipientId)";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, message);
    }

    public async Task UpdateMessageAsync(MessageDto message)
    {
        const string query = "UPDATE messages SET message_content = @MessageContent, sent_date = @SentDate, sender_id = @SenderId, recipient_id = @RecipientId WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, message);
    }

    public async Task DeleteMessageAsync(int id)
    {
        const string query = "DELETE FROM messages WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, new { Id = id });
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesBetweenTwoUsers(Guid senderId, Guid receiverId)
    {
        const string query = "SELECT id, message_content, sent_date, sender_id, recipient_id FROM messages WHERE (sender_id = @SenderId AND recipient_id=@ReceiverId) OR (sender_id = @ReceiverId AND recipient_id=@SenderId)";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<MessageDto>(query, new { SenderId = senderId, ReceiverId = receiverId });
    }
}