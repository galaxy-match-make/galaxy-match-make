using System.ComponentModel.DataAnnotations.Schema;

namespace galaxy_match_make.Models;

[Table("profile_attributes")]
public class ProfileAttributesDto
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int CharacteristicId { get; set; }
}
