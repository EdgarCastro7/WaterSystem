using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public interface IConsumptionRepository : IGenericRepository<Consumption>
    {
        Task<IEnumerable<Consumption>> GetConsumptionAsync(Meter meter);

        Task<Consumption> GetLastConsumptionAsync(Meter meter);

        Task<bool> AlreadyExistsAsync(Meter meter, DateTime date);
    }
}
