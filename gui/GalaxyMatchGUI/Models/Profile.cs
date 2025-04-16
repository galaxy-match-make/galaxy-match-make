using System;
using System.Collections.Generic;

namespace GalaxyMatchGUI.Models
{
    public class Profile
    {
        public Profile()
        {
            UserInterests = new List<UserInterest>();
        }
        
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public int? SpeciesId { get; set; }
        public int? PlanetId { get; set; }
        public int? GenderId { get; set; }
        public float? HeightInGalacticInches { get; set; }
        public int? GalacticDateOfBirth { get; set; }
        
        // Navigation properties
        public User? User { get; set; }
        public Species? Species { get; set; }
        public Planet? Planet { get; set; }
        public Gender? Gender { get; set; }
        public List<UserInterest> UserInterests { get; set; }
    }
}