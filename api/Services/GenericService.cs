using galaxy_match_make.Repositories;

namespace galaxy_match_make.Services;

public class GenericService<T>(IGenericRepository<T> repository) : IGenericService<T> where T : class
{
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(T entity)
    {
        return await repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        return await repository.UpdateAsync(entity);
    }
}