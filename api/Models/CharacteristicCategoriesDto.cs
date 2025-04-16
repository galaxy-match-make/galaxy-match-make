using System.ComponentModel.DataAnnotations.Schema;

namespace galaxy_match_make.Models;

[Table("characteristic_categories")]
public class CharacteristicCategoriesDto
{
    public int Id { get; set; }
    public string CharacteristicsCategoryName { get; set; } = null!;
}
