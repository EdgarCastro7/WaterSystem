using WaterSystem.Data.Entities;

namespace WaterSystem.Models
{
    public class InvoiceDetailsViewModel
    {
        public Invoice Invoice { get; set; }

        public decimal? PreviousReading { get; set; }

        public decimal CurrentReading { get; set; }

        public decimal ConsumptionVolume { get; set; }

        public List<InvoicePriceBreakdownViewModel> PriceBreakdown { get; set; } = new();

        public decimal TotalPrice { get; set; }
    }

    public class InvoicePriceBreakdownViewModel
    {
        public string Tier { get; set; }

        public decimal Volume { get; set; }

        public decimal PricePerM3 { get; set; }

        public decimal Subtotal { get; set; }
    }
}
