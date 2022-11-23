using Indra.Net;
using Indra.Net.Focuses.Actors;
using Indra.Net.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Indra.Api.Configuration {
  public class DbContext : Microsoft.EntityFrameworkCore.DbContext {

    public DbSet<Server> Servers { get; private set; }

    public DbSet<User> Users { get; private set; }

    public DbSet<Entity> Entities { get; private set; }

    public DbSet<Character> Characters { get; private set; }

    public DbSet<Place> Places { get; private set; }

    public DbSet<World> Worlds { get; private set; }

    public DbSet<Zone> Zones { get; private set; }

    public DbSet<Room> Rooms { get; private set; }

    public DbSet<Area> Areas { get; private set; }

    public DbSet<Navigation> Navigations { get; private set; }

    public DbSet<Permission> PermissionTypes { get; private set; }

    public DbSet<GrantedPermission> GrantedPermissions { get; private set; }

    public DbContext(DbContextOptions dbContextOptions)
      : base(dbContextOptions) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<Server>()
        .HasMany<World>("Worlds")
        .WithOne(w => w.Server);

      modelBuilder.Entity<World>()
        .HasIndex(w => new {
          w.Key,
          w.Server
        }).IsUnique()
        .IsClustered();
    }
  }
}
