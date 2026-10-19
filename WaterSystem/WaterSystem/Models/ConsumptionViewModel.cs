using System.ComponentModel.DataAnnotations;

namespace WaterSystem.Models
{
    public class ConsumptionViewModel
    {
        public int Id { get; set; }

        [Required]
        public int MeterId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "The meter reading must be higher than zero.")]
        public decimal Volume { get; set; }
    }
}
