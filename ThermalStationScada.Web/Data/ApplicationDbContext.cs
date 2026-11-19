using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ThermalStationScada.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SensorSample> SensorSamples => Set<SensorSample>();
        public DbSet<LoginEvent> LoginEvents => Set<LoginEvent>();
    }
}