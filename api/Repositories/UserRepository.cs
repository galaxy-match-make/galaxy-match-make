using Dapper;
using galaxy_match_make.Data;
using galaxy_match_make.Models;
using Npgsql;


namespace galaxy_match_make.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _context;

        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            using var connection = _context.CreateConnection();
            var users = await connection.QueryAsync<UserDto>("SELECT * FROM USERS");
            return users.ToList();
        }

        public async Task AddUser(string oauthId)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteScalarAsync<Guid>(
                @"INSERT INTO users (oauth_id) 
                      VALUES (@OAuthId) 
                      RETURNING id",
                new { OAuthId = oauthId });
        }
        
        public async Task<UserDto> GetUserByOauthId(string oauthId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<UserDto>(
                "SELECT id, oauth_id AS OAuthId, inactive FROM users WHERE oauth_id = @OAuthId LIMIT 1",
                new { OAuthId = oauthId });
        }
        
        public async Task<UserDto> GetUserById(Guid id)
        {
            using var connection = _context.CreateConnection();
            var user = await connection
                .QueryFirstOrDefaultAsync<UserDto>("SELECT * FROM USERS WHERE ID = @Id", new { Id = id });

            return user;
        }
        
        public async Task DeleteUser(Guid id)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("UPDATE users SET inactive = true WHERE id = @Id", new { Id = id });
        }

        public async Task UpdateUser(UserDto user)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE users SET oauth_id = @OAuthId, inactive = @Inactive WHERE id = @Id", 
                new { Id = user.Id, OAuthId = user.OAuthId, Inactive = user.Inactive });
        }
    }
}
