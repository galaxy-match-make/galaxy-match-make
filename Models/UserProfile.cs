namespace galaxy_match_make.Models
{
    public class UserProfile
    {
        public int ProfileId { get; set; }

        public required int UserId { get; set; }

        public required int SpeciesId { get; set; }

        public string? Bio { get; set; }

        public string? HomePlanet { get; set; }

        public int AgeInEarthYears { get; set; }
    }
}
