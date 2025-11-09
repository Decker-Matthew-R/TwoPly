using Microsoft.EntityFrameworkCore;
using TwoPly.Teams;

namespace TwoPly.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Team> Teams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TeamName)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.CreatedDate)
                .IsRequired();
        });
    }
}