using System.ComponentModel.DataAnnotations;

namespace WaterSystem.Data.Entities
{
    public class MeterRequest : IEntity
    {
        public int Id { get; set; }

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
        public string Address { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime RequestDate { get; set; } = DateTime.Today;

        [Required]
        public string Status { get; set; } = "Pending";
    }
}
