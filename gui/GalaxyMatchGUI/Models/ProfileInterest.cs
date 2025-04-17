namespace GalaxyMatchGUI.Models
{
    public class ProfileInterest
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int InterestId { get; set; }
        
        public required Profile Profile { get; set; }
        public required Interest Interest { get; set; }
    }
}