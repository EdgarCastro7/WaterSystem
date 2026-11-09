using System.ComponentModel.DataAnnotations;

namespace WaterSystem.Models
{
    public class AdditionalMeterRequestViewModel
    {
        [Required]
        [Display(Name = "Installation Address")]
        public string Address { get; set; } = string.Empty;
    }
}
