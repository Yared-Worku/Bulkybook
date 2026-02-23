using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bulkybookweb.Models
{
    // EF Core will use this to create the DbContext at design-time
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Replace with your local PostgreSQL connection string for migrations
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=bulkybook;Username=postgres;Password=YourPassword");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}