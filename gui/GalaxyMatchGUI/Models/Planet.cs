using System.Collections.Generic;

namespace GalaxyMatchGUI.Models
{
    public class Planet
    {
        public Planet()
        {
            Profiles = new HashSet<Profile>();
        }
        
        public int Id { get; set; }
        public required string PlanetName { get; set; }
        
        public ICollection<Profile> Profiles { get; set; }
    }
}