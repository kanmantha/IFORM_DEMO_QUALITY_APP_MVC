using IFormQualityApp.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<SiteQuery> SiteQueries => Set<SiteQuery>();

    public DbSet<QueryComment> QueryComments => Set<QueryComment>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<EotRequest> EotRequests => Set<EotRequest>();

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SiteQuery>()
            .HasIndex(q => q.QueryNumber).IsUnique();
        builder.Entity<SiteQuery>()
            .HasIndex(q => q.IPO);
        builder.Entity<SiteQuery>()
            .HasIndex(q => new { q.Status, q.IssueType });

        builder.Entity<Project>()
            .HasIndex(p => p.Name).IsUnique();

        builder.Entity<Product>()
            .HasIndex(p => p.Code).IsUnique();

        builder.Entity<EotRequest>()
            .HasIndex(e => e.EotNumber).IsUnique();

        builder.Entity<QueryComment>()
            .HasOne(c => c.Query).WithMany(q => q.Comments)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AuditLog>()
            .HasOne(a => a.Query).WithMany(q => q.AuditLogs)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SiteQuery>()
            .HasOne(q => q.RaisedBy)
            .WithMany()
            .HasForeignKey(q => q.RaisedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SiteQuery>()
            .HasOne(q => q.ResolvedBy)
            .WithMany()
            .HasForeignKey(q => q.ResolvedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QueryComment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EotRequest>()
            .HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
