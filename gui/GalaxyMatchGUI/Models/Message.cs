using System;
public class Message
{
    public int Id { get; set; }
    public string MessageContent { get; set; }
    public DateTime SentDate { get; set; } = DateTime.UtcNow;
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
}
