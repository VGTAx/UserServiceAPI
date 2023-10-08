using Microsoft.EntityFrameworkCore;
using UserServiceAPI.Models;

namespace UserServiceAPI.Data
{
  /// <summary>
  /// The data context for the UserService.
  /// </summary>
  public class UserServiceContext : DbContext
    {
    /// <summary>
    /// UserServiceContext constructor
    /// </summary>
    /// <param name="options">Data context configuration options.</param>
    public UserServiceContext(DbContextOptions<UserServiceContext> options) : base(options) { }
    /// <summary>
    /// User data set
    /// </summary>
    public DbSet<User> Users { get; set; }
    /// <summary>
    /// Role data set
    /// </summary>
    public DbSet<Role> Roles { get; set; }
    /// <summary>
    /// UserRoles data set
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder
        .Entity<UserRole>()
        .HasKey(ur => new { ur.UserId, ur.RoleId });

      modelBuilder
        .Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();

      modelBuilder
        .Entity<User>()
        .HasMany(u => u.UserRoles)
        .WithOne(ur => ur.User)
        .HasForeignKey(ur => ur.UserId);

      modelBuilder.Entity<User>()
        .Ignore(u => u.Roles);

      modelBuilder
        .Entity<Role>()
        .HasMany(u => u.UserRoles)
        .WithOne(ur => ur.Role)
        .HasForeignKey(ur => ur.RoleId);

      modelBuilder
        .Entity<Role>()
        .HasData(
            new Role { Id = 1, RoleName = "User" },
            new Role { Id = 2, RoleName = "Support" },
            new Role { Id = 3, RoleName = "Admin" },
            new Role { Id = 4, RoleName = "SuperAdmin" }
          );
      modelBuilder
        .Entity<User>()
        .HasData(
            new User { Id = 1, Name = "Ivan Alexeev", Age = 32, Email = "i.alexeev@gmail.com" },
            new User { Id = 2, Name = "Oleg Andreev", Age = 22, Email = "o.andreev@gmail.com" },
            new User { Id = 3, Name = "Olga Petrova", Age = 24, Email = "o.petrova@gmail.com" },
            new User { Id = 4, Name = "Elena Ivanova", Age = 29, Email = "e.ivanova@gmail.com" },
            new User { Id = 5, Name = "Pavel Durov", Age = 38, Email = "p.durov@telegram.com" },
            new User { Id = 6, Name = "Elon Musk", Age = 52, Email = "e.musk@spaceX.com" },
            new User { Id = 7, Name = "Bill Gates", Age = 67, Email = "b.gates@microsoft.com"},
            new User { Id = 8, Name = "Tim Cook", Age = 62, Email = "t.cook@apple.com" },
            new User { Id = 9, Name = "Mark Zuckerberg", Age = 39, Email = "m.zuckerberg@meta.com" }
          );
      modelBuilder
        .Entity<UserRole>()
        .HasData(
            new UserRole { UserId = 1, RoleId = 1 },
            new UserRole { UserId = 2, RoleId = 1 },
            new UserRole { UserId = 3, RoleId = 1 },
            new UserRole { UserId = 4, RoleId = 1 },
            new UserRole { UserId = 5, RoleId = 3 },
            new UserRole { UserId = 5, RoleId = 4 }, 
            new UserRole { UserId = 6, RoleId = 2 },
            new UserRole { UserId = 7, RoleId = 3 },
            new UserRole { UserId = 7, RoleId = 4 },
            new UserRole { UserId = 8, RoleId = 2 },
            new UserRole { UserId = 9, RoleId = 2 },
            new UserRole { UserId = 9, RoleId = 3 }
          );
    }    
  }
}
