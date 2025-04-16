using System.ComponentModel.DataAnnotations.Schema;

namespace galaxy_match_make.Models;

[Table("characteristics")]
public class CharacteristicsDto
{
    public int Id { get; set; }
    [ForeignKey("characteristic_categories_id")]
    public int CharacteristicCategoriesId { get; set; }
    public string CharacteristicsName { get; set; } = null!;
}
