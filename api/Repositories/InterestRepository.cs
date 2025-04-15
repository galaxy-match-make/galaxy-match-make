using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;

namespace galaxy_match_make.Repositories
{
    public class InterestRepository : IInterestRepository
    {
        private readonly DapperContext _context;
        
        public InterestRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<InterestDto>> GetAllInterestsAsync()
        {
            var query = "SELECT id, interest_name AS InterestName FROM interests";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InterestDto>(query);
        }

        public async Task<InterestDto> GetInterestByIdAsync(int id)
        {
            var query = "SELECT id, interest_name AS InterestName FROM interests WHERE id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<InterestDto>(query, new { Id = id });
        }
    }
}