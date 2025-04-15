using System.ComponentModel.DataAnnotations.Schema;

namespace galaxy_match_make.Models;

[Table("profile_preferences")]
public class ProfilePreferencesDto
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int CharacteristicId { get; set; }
}
