using Microsoft.EntityFrameworkCore;
using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public class ConsumptionRepository : GenericRepository<Consumption>, IConsumptionRepository
    {
        private readonly DataContext _context;

        public ConsumptionRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> AlreadyExistsAsync(Meter meter, DateTime date)
        {
            var exists = await _context.Consumptions
                .Where(me => me.MeterId == meter.Id)
                .Where(m => m.Date == date)
                .AnyAsync();

            return exists;
        }

        public async Task<IEnumerable<Consumption>> GetConsumptionsAsync(Meter meter)
        {
            var consumptions = await _context.Consumptions
                .Where(c => c.MeterId == meter.Id)
                .ToListAsync();

            return consumptions;
        }

        public async Task<IEnumerable<Consumption>> GetConsumptionsAsync()
        {
            var consumptions = await _context.Consumptions
                .Include(c => c.Meter)
                .ToListAsync();

            return consumptions;
        }

        public async Task<Consumption> GetLastConsumptionAsync(Meter meter)
        {
            var consumption = await _context.Consumptions
                .Where(m => m.MeterId == meter.Id)
                .OrderByDescending(c => c.Date)
                .FirstOrDefaultAsync();

            return consumption;
        }
    }
}
