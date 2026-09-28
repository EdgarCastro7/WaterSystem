using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WaterSystem.Data.Entities
{
    public class Invoice : IEntity
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public decimal Price { get; set; }

        public bool Payment {  get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public int MeterId { get; set; }

        public Meter Meter { get; set; }

        public int ConsumptionId {get; set;}

        public Consumption Consumption { get; set; }    
    }
}
