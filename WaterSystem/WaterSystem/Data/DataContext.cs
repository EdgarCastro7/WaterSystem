using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> dbContext) : base(dbContext)
        {
            
        }
    }
}
