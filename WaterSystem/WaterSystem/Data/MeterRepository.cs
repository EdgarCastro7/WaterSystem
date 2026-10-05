using Microsoft.EntityFrameworkCore;
using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public class MeterRepository : GenericRepository<Meter>, IMeterRepository
    {
        private readonly DataContext _context;

        public MeterRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Meter>> GetMeterAsync(User user)
        {
            var meter = await _context.Meters.Where(m => m.UserId == user.Id).ToListAsync();

            return meter;
        }
    }
}
