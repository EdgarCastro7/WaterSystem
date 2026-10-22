using System.ComponentModel.DataAnnotations;

namespace WaterSystem.Models
{
    public class MeterRequestViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Installation Address")]
        public string Address { get; set; } = null!;
    }
}
