namespace WaterSystem.Data.Entities
{
    public class Meter : IEntity
    {
        public int Id { get; set; }

        public DateTime InstallationDate { get; set; }

        public bool IsActive { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public ICollection<Consumption> Consumptions { get; set; }
    }
}
