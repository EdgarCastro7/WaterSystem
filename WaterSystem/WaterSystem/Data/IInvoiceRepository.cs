using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetInvoicesAsync();

        Task<IEnumerable<Invoice>> GetInvoicesByUserAsync(User user);

        Task<IEnumerable<Invoice>> GetInvoicesByPeriodAsync(DateTime startDate, DateTime endDate);

        Task<IEnumerable<Invoice>> GetInvoicesByPaymentAsync(bool payment);

        Task<Invoice?> GetLastInvoiceAsync(User user);

        Task<Invoice?> GetLastInvoiceToPayAsync(User user);
    }
}
