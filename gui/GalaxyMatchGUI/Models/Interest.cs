using System.Collections.Generic;

namespace GalaxyMatchGUI.Models
{
    public class Interest
    {
        public Interest()
        {
            ProfileInterests = new HashSet<ProfileInterest>();
        }
        
        public int Id { get; set; }
        public required string InterestName { get; set; }
        
        public ICollection<ProfileInterest> ProfileInterests { get; set; }
    }
}