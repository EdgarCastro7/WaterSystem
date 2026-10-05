using Microsoft.EntityFrameworkCore;
using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        private readonly DataContext _context;

        public InvoiceRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesAsync()
        {
            var invoices = await _context.Invoices.ToListAsync();

            return invoices;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByPaymentAsync(bool payment)
        {
            var invoices = await _context.Invoices
            .Where(i => i.Payment == payment)
            .ToListAsync();

            return invoices;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var invoices = await _context.Invoices
            .Where(i => i.DateTime >= startDate && i.DateTime <= endDate)
            .ToListAsync();

            return invoices;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByUserAsync(User user)
        {
            var invoices = await _context.Invoices.Where(i => i.UserId == user.Id).ToListAsync();

            return invoices;
        }

        public async Task<Invoice?> GetLastInvoiceAsync(User user)
        {
            var invoice = await _context.Invoices
                .Where(i => i.UserId == user.Id)
                .OrderByDescending(i => i.DateTime)
                .FirstOrDefaultAsync();

            return invoice;
        }

        public async Task<Invoice?> GetLastInvoiceToPayAsync(User user)
        {
            var invoice = await _context.Invoices
            .Where(i => i.UserId == user.Id && i.Payment == false)
            .OrderByDescending(i => i.DateTime)
            .FirstOrDefaultAsync();

            return invoice;
        }
    }
}
