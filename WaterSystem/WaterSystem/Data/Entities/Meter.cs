using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WaterSystem.Data.Entities
{
    public class Meter : IEntity
    {
        public int Id { get; set; }

        public DateTime InstallationDate { get; set; }

        public bool IsActive { get; set; }

        public string UserId { get; set; }

        [ValidateNever]
        public User User { get; set; }

        [ValidateNever]
        public ICollection<Consumption> Consumptions { get; set; }
    }
}
