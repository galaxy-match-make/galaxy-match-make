using System.ComponentModel.DataAnnotations.Schema;

namespace galaxy_match_make.Models;

[Table("characteristics")]
public class CharacteristicsDto
{
    public int Id { get; set; }
    public string CharacteristicsCategory { get; set; } = null!;
    public string CharacteristicsName { get; set; } = null!;
}
