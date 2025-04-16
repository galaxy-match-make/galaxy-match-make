using System;
using System.Text.Json.Serialization;

namespace GalaxyMatchGUI.Models
{
    public class UserInterest
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int InterestId { get; set; }
        public string InterestName { get; set; } = string.Empty;
        
        // Navigation properties - ignore these during serialization/deserialization
        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Interest? Interest { get; set; }
    }
}