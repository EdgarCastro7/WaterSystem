using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public interface IGenericRepository<T> where T : class, IEntity
    {
        IQueryable<T> GetAll();

        Task<T> GetByIdAsync(int id);

        Task CreateAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<bool> ExistAsync(int id);
    }
}
