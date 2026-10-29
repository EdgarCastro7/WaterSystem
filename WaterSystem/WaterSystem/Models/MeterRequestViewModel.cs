using System.ComponentModel.DataAnnotations;

namespace WaterSystem.Models
{
    public class MeterRequestViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Installation Address")]
        public string Address { get; set; } = null!;
    }
}
