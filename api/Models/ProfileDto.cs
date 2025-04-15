using System.ComponentModel.DataAnnotations.Schema;

namespace galaxy_match_make.Models
{
    [Table("profiles")]
    public class ProfileDto
    {
        public int Id { get; set; }
        public required Guid UserId { get; set; }
        public required string DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }

        public float? HeightInGalacticInches { get; set; }
        public int? GalacticDateOfBirth { get; set; }

        public SpeciesDto? Species { get; set; }
        public PlanetDto? Planet { get; set; }
        public GenderDto? Gender { get; set; }

        public List<UserInterestsDto> UserInterests { get; set; } = new List<UserInterestsDto>();

    }

    public class UpdateProfileDto
    {
        public string DisplayName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public float? HeightInGalacticInches { get; set; }
        public int? GalacticDateOfBirth { get; set; }
        public int? SpeciesId { get; set; }
        public int? PlanetId { get; set; }
        public int? GenderId { get; set; }
        public List<int>? UserInterestIds { get; set; }
    }

    public class CreateProfileDto
    {
        public string DisplayName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public float? HeightInGalacticInches { get; set; }
        public int? GalacticDateOfBirth { get; set; }
        public int? SpeciesId { get; set; }
        public int? PlanetId { get; set; }
        public int? GenderId { get; set; }
        public List<int>? UserInterestIds { get; set; }
    }
}
