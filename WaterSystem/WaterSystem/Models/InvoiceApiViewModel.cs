namespace WaterSystem.Models
{
    public class InvoiceApiViewModel
    {
        public int InvoiceId { get; set; }

        public DateTime DateTime { get; set; }

        public decimal Price { get; set; }

        public string Status { get; set; }

        public bool Payment { get; set; }

        public int MeterId { get; set; }

        public int ConsumptionId { get; set; }
    }
}
