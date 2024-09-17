namespace IncidentFinder.Data.Context;

using IncidentFinder.Data.Entities;
using Microsoft.EntityFrameworkCore;

public class Context : DbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<Incident> Incidents { get; set; }

    public Context(DbContextOptions<Context> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>().ToTable("events");
        modelBuilder.Entity<Incident>().ToTable("incidents");

        modelBuilder.Entity<Incident>()
            .HasMany(i => i.Events)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
