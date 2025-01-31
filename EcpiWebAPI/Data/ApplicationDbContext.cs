using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;  // 🔹 Add this to ensure UserTable is recognized

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<UserTable> Users { get; set; } // 🔹 Ensure this matches your UserTable class

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserTable>().ToTable("UserTable");
    }
}
