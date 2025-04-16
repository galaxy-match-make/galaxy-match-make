using System;
using System.Collections.Generic;

namespace GalaxyMatchGUI.Models
{
    public class Profile
    {
        public Profile()
        {
            // UserInterests = new HashSet<UserInterest>();
        }
        
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public required string DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        // public int SpeciesId { get; set; }
        // public int PlanetId { get; set; }
        // public int? GenderId { get; set; }
        // public float? HeightInGalacticInches { get; set; }
        // public int? GalacticDateOfBirth { get; set; }
        
        // Navigation properties
        public User? User { get; set; }  // Made optional by removing required keyword and adding nullable
        // public required Species Species { get; set; }
        // public required Planet Planet { get; set; }
        // public Gender? Gender { get; set; }
        // public ICollection<UserInterest> UserInterests { get; set; }
    }
}