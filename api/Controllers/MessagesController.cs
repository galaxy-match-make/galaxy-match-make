using System.Security.Claims;
using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _messageRepository;

    public MessagesController(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMessages()
    {
        var messages = await _messageRepository.GetAllMessagesAsync();
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMessageById(int id)
    {
        var message = await _messageRepository.GetMessageByIdAsync(id);
        if (message == null)
        {
            return NotFound();
        }
        return Ok(message);
    }

    [HttpPost]
    public async Task<IActionResult> AddMessage([FromBody] MessageDto message)
    {
        await _messageRepository.AddMessageAsync(message);
        return CreatedAtAction(nameof(GetMessageById), new { id = message.Id }, message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMessage(int id, [FromBody] MessageDto message)
    {
        if (id != message.Id)
        {
            return BadRequest();
        }

        await _messageRepository.UpdateMessageAsync(message);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        await _messageRepository.DeleteMessageAsync(id);
        return NoContent();
    }

    [HttpGet("between")]
    public async Task<IActionResult> GetMessagesBetweenTwoUsers([FromQuery] Guid senderId, [FromQuery] Guid receiverId)
    {
        var messages = await _messageRepository.GetMessagesBetweenTwoUsers(senderId, receiverId);

        return Ok(messages);
    }

}