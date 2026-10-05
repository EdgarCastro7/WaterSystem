using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public interface IMeterRepository : IGenericRepository<Meter>
    {
        Task<IEnumerable<Meter>> GetMeterAsync(User user);
    }
}
