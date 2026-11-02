using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public class PriceListRepository : GenericRepository<PriceList>, IPriceListRepository
    {
        private readonly DataContext _context;

        public PriceListRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PriceList>> GetPriceListsAsync()
        {
            var priceLists = await _context.PriceLists
                .OrderBy(p => p.MaxVolume == null)
                .ThenBy(p => p.MaxVolume)
                .ToListAsync();

            return priceLists;
        }
    }
}
