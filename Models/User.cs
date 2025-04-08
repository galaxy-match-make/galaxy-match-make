namespace galaxy_match_make.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string GoogleId { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public DateTime RegistrationDate { get; set; }

        public UserProfile? Profile { get; set; }

    }
}
