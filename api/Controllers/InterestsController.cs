using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterestsController : ControllerBase
    {
        private readonly IInterestRepository _interestRepository;

        public InterestsController(IInterestRepository interestRepository) => _interestRepository = interestRepository;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InterestDto>>> GetAllInterests()
        {
            var interests = await _interestRepository.GetAllInterestsAsync();
            return Ok(interests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InterestDto>> GetInterestById(int id)
        {
            var interest = await _interestRepository.GetInterestByIdAsync(id);
            if (interest == null)
            {
                return NotFound("Interest not found");
            }
            return Ok(interest);
        }
    }
}