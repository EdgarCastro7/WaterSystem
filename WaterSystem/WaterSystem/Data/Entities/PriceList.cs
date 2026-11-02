using System.ComponentModel.DataAnnotations;

namespace WaterSystem.Data.Entities
{
    public class PriceList : IEntity
    {
        public int Id { get; set; }

        public decimal? MaxVolume { get; set; }

        [Required]
        public decimal PricePerM3 { get; set; }
    }
}
