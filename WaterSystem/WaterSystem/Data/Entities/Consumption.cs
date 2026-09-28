namespace WaterSystem.Data.Entities
{
    public class Consumption : IEntity
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public decimal Volume { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }
    }
}
