using Dapper;
using galaxy_match_make.Models;
using Npgsql;

namespace galaxy_match_make.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;

        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_configuration
                .GetConnectionString("DefaultConnection"));
        }

        public async Task<List<User>> GetAllUsers()
        {
            // using var connection = GetConnection();
            //  var users = await connection.QueryAsync<User>("SELECT * FROM USERS");
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    GoogleId = "g_108742956835221095142",
                    Email = "zorgblatt@gmail.com",
                    Username = "CommanderZorg",
                    RegistrationDate = new DateTime(2024, 12, 15)
                },
                new User
                {
                    Id = 2,
                    GoogleId = "g_115839274601938472651",
                    Email = "kl33x@gmail.com",
                    Username = "StarDustKl33x",
                    RegistrationDate = new DateTime(2025, 1, 23)
                },
                new User
                {
                    Id = 3,
                    GoogleId = "g_103628471905237461829",
                    Email = "nebulon5@gmail.com",
                    Username = "NebulaNavigator",
                    RegistrationDate = new DateTime(2025, 3, 7)
                }
            };
            return users;
        }
        async Task<User> IUserRepository.GetUserById(int id)
        {
            //using var connection = GetConnection();
            //connection.Open();
            //var user = await connection
            //    .QueryFirstOrDefaultAsync<User>("SELECT FROM USERS WHERE USER_ID= @Id", new {Id = id});
            var user = new User
            {
                Id = 1,
                GoogleId = "g_108742956835221095142",
                Email = "zorgblatt@gmail.com",
                Username = "CommanderCindi",
                RegistrationDate = new DateTime(2024, 12, 15)
            };
            return user;
        }

        Task IUserRepository.AddUser(User user)
        {
            throw new NotImplementedException();
        }

        Task IUserRepository.DeleteUser(int id)
        {
            throw new NotImplementedException();
        }

        Task IUserRepository.UpdateUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
