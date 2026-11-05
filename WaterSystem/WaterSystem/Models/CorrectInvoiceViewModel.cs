using System.ComponentModel.DataAnnotations;

namespace WaterSystem.Models
{
    public class CorrectInvoiceViewModel
    {
        public int OriginalInvoiceId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Enter a valid invoice value.")]
        public decimal? NewPrice { get; set; }
    }
}
