using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public interface IConsumptionRepository : IGenericRepository<Consumption>
    {
        Task<IEnumerable<Consumption>> GetConsumptionsAsync();

        Task<IEnumerable<Consumption>> GetConsumptionsAsync(Meter meter);

        Task<Consumption> GetLastConsumptionAsync(Meter meter);

        Task<bool> AlreadyExistsAsync(Meter meter, DateTime date);
    }
}
