namespace BlogCore.DAL.Data;

using BlogCore.DAL.Models;
using Microsoft.EntityFrameworkCore;

public class BlogContext : DbContext
{
    // Konstruktor pozwalaj¹cy na wstrzykniêcie konfiguracji (np. z Testcontainers) 
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
    }

    // Definicje tabel w bazie danych
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracja modelu Post
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();

            entity.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

