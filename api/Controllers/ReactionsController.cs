using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace galaxy_match_make.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReactionsController : ControllerBase
{
    private readonly IReactionRepository _reactionRepository;
    private readonly IUserRepository _userRepository;

    public ReactionsController(IReactionRepository reactionRepository, IUserRepository userRepository)
    {
        _reactionRepository = reactionRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReactions()
    {
        var reactions = await _reactionRepository.GetAllReactionsAsync();
        return Ok(reactions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReactionById(int id)
    {
        var reaction = await _reactionRepository.GetReactionByIdAsync(id);
        if (reaction == null)
        {
            return NotFound();
        }
        return Ok(reaction);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReaction([FromBody] ReactionRequest request)
    {
        var reactorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(reactorIdClaim) || !Guid.TryParse(reactorIdClaim, out Guid reactorId))
        {
            return Unauthorized("User ID not found in token or invalid format");
        }

        var reactor = await _userRepository.GetUserById(reactorId);
        if (reactor == null)
        {
            return NotFound($"Reactor user with ID {reactorId} not found");
        }

        var target = await _userRepository.GetUserById(request.TargetId);
        if (target == null)
        {
            return NotFound($"Target user with ID {request.TargetId} not found");
        } 
        else if (target.Id == reactorId)
        {
            return BadRequest("You cannot react to your own profile.");
        }

        var reaction = new ReactionDto
        {
            ReactorId = reactorId,
            TargetId = request.TargetId,
            IsPositive = request.IsPositive
        };
        
        var existingReaction = await _reactionRepository.GetReactionByReactorAndTargetAsync(reaction.ReactorId, reaction.TargetId);
        if (existingReaction != null)
        {
            return BadRequest("You have already reacted to this profile.");
        }
        else
        {
            await _reactionRepository.AddReactionAsync(reaction);
            return CreatedAtAction(nameof(GetReactionById), new { id = reaction.Id }, reaction);
        }

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReaction(int id, [FromBody] ReactionDto reaction)
    {
        if (id != reaction.Id)
        {
            return BadRequest();
        }
        await _reactionRepository.UpdateReactionAsync(reaction);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReaction(int id)
    {
        await _reactionRepository.DeleteReactionAsync(id);
        return NoContent();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetReactionsByReactor(Guid userId)
    {
        var reactions = await _reactionRepository.GetReactionsByReactorAsync(userId);
        return Ok(reactions);
    }
}

public class ReactionRequest
{
    public Guid TargetId { get; set; }
    public bool IsPositive { get; set; }
}