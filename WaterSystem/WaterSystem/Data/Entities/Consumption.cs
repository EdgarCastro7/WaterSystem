namespace WaterSystem.Data.Entities
{
    public class Consumption : IEntity
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public decimal Volume { get; set; }

        public int MeterId { get; set; }

        public Meter Meter { get; set; }
    }
}
