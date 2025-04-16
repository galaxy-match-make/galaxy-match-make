using galaxy_match_make.Models;
using galaxy_match_make.Repositories;

namespace galaxy_match_make.Services;

public class CharacteristicsService(
    IGenericRepository<CharacteristicsDto> characteristicsRepository, 
    IGenericRepository<CharacteristicCategoriesDto> characteristicCategoriesRepository) 
    : GenericService<CharacteristicsDto>(characteristicsRepository), ICharacteristicsService
{
    public async Task<Dictionary<CharacteristicCategoriesDto, List<CharacteristicsDto>>> GetCharacteristicsGroupedByCategory(List<CharacteristicsDto>? characteristics = null)
    {
        Dictionary<CharacteristicCategoriesDto, List<CharacteristicsDto>> characteristicsGroupedByCategory = new Dictionary<CharacteristicCategoriesDto, List<CharacteristicsDto>>();
        
        List<CharacteristicCategoriesDto> characteristicCategories =
            (await characteristicCategoriesRepository.GetAllAsync()).ToList();
        
        characteristics ??= (await characteristicsRepository.GetAllAsync()).ToList();

        foreach (CharacteristicCategoriesDto category in characteristicCategories)
        {
            characteristicsGroupedByCategory.Add(
                category, 
                characteristics
                    .Where(characteristic => characteristic.CharacteristicCategoriesId == category.Id)
                    .ToList());
        }

        return characteristicsGroupedByCategory;
    }
}