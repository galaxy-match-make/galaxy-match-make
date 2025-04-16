using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GalaxyMatchGUI.Models
{
    public class Gender
    {
        public Gender()
        {
            Profiles = new HashSet<Profile>();
        }
        
        public int Id { get; set; }
        
        // Adding the JsonPropertyName to handle the API property name
        [JsonPropertyName("gender")]
        public string GenderName { get; set; } = string.Empty;
        
        // Navigation property
        [JsonIgnore]
        public ICollection<Profile> Profiles { get; set; }
    }
}