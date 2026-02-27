using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bulkybookweb.Models
{
    public class ApplicationDbContext : IdentityDbContext<Users, Roles, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Category>(entity => {
                entity.HasKey(e => e.CategoryCode);
                entity.Property(e => e.CategoryCode).HasColumnName("Category_Code");
            });
            builder.Entity<Users>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.Id).HasColumnName("UserId");
                entity.Property(e => e.FName).HasColumnName("FName");
                entity.Property(e => e.LName).HasColumnName("LName");
                entity.Property(e => e.Is_Active).HasColumnName("Is_Active");
                entity.Property(e => e.Created_by).HasColumnName("Created_by");
                entity.Property(e => e.Created_date).HasColumnName("Created_date");
            });

            builder.Entity<Roles>(entity =>
            {
                entity.ToTable("AspNetRoles");
                entity.Property(e => e.Id).HasColumnName("RoleId");
                entity.Property(e => e.Created_by).HasColumnName("Created_by");
                entity.Property(e => e.Created_date).HasColumnName("Created_date");
            });
            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("AspNetUserRoles");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.RoleId).HasColumnName("RoleId");
            });
            builder.Entity<IdentityUserClaim<Guid>>(entity => entity.ToTable("AspNetUserClaims"));

            builder.Entity<IdentityUserLogin<Guid>>(entity => entity.ToTable("AspNetUserLogins"));
            builder.Entity<IdentityRoleClaim<Guid>>(entity => entity.ToTable("AspNetRoleClaims"));
            builder.Entity<IdentityUserToken<Guid>>(entity => entity.ToTable("AspNetUserTokens"));
        }
    }
}