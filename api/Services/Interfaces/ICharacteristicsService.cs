using galaxy_match_make.Models;

namespace galaxy_match_make.Services;

public interface ICharacteristicsService : IGenericService<CharacteristicsDto>
{
    Task<Dictionary<CharacteristicCategoriesDto, List<CharacteristicsDto>>> GetCharacteristicsGroupedByCategory(List<CharacteristicsDto>? characteristics = null);
}