namespace galaxy_match_make.Controllers;

using System.Security.Claims;
using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class InteractionsController : ControllerBase
{
    private readonly IInteractionRepository _interactionRepository;
    
    public InteractionsController(IInteractionRepository interactionRepository)
    {
        _interactionRepository = interactionRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetReactions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var connections = await _interactionRepository.GetReactions(Guid.Parse(userId));

        return Ok(connections);

    }
    
    [HttpGet("received-requests")]
    [Authorize]
    public async Task<IActionResult> GetReceivedRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var requests = await _interactionRepository.GetReceivedRequests(Guid.Parse(userId));

        return Ok(requests);
    }
    
    [HttpGet("sent-requests")]
    [Authorize]
    public async Task<IActionResult> GetSentRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var requests = await _interactionRepository.GetSentRequests(Guid.Parse(userId));

        return Ok(requests);

    }
    
    [HttpDelete("cancel-request/{targetId}")]
    public async Task<IActionResult> CancelRequest(Guid targetId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    
        try
        {
            await _interactionRepository.CancelRequest(userId, targetId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error cancelling request: {ex.Message}");
        }
    }

    [HttpPost("accept-request/{targetId}")]
    public async Task<IActionResult> AcceptRequest(Guid targetId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    
        try
        {
            await _interactionRepository.ReactToRequest(userId, targetId, true);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error accepting request: {ex.Message}");
        }
    }

    [HttpPost("reject-request/{targetId}")]
    public async Task<IActionResult> RejectRequest(Guid targetId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    
        try
        {
            await _interactionRepository.ReactToRequest(userId, targetId, false);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error rejecting request: {ex.Message}");
        }
    }

}