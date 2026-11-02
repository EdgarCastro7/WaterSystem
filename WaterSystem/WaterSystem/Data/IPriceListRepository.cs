using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public interface IPriceListRepository : IGenericRepository<PriceList>
    {
        Task<IEnumerable<PriceList>> GetPriceListsAsync();
    }
}
