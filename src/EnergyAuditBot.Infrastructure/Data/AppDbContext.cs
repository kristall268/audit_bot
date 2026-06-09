using EnergyAuditBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnergyAuditBot.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<HousingProfile> HousingProfiles => Set<HousingProfile>();
    public DbSet<Appliance> Appliances => Set<Appliance>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<User>(e =>
        {
            e.HasKey(u => u.ChatId);
            e.Property(u => u.ChatId).ValueGeneratedNever();
        });

        mb.Entity<HousingProfile>(e =>
        {
            mb.Entity<HousingProfile>().HasKey(p => p.Id);
            
            e.HasOne(p => p.User)
             .WithOne(u => u.Profile)
             .HasForeignKey<HousingProfile>(p => p.UserId);
        });

        mb.Entity<Appliance>(e =>
        {
            mb.Entity<Appliance>().HasKey(a => a.Id);

            e.HasOne(a => a.Profile)
             .WithMany(p => p.Appliances)
             .HasForeignKey(a => a.ProfileId);
        });
    }
}