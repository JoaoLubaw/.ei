using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pontuei.Api.Models;

namespace Pontuei.Api.Data;

[ExcludeFromCodeCoverage]
public class PontueiDbContext(DbContextOptions<PontueiDbContext> options) : DbContext(options)
{
    public required DbSet<Configuration> Configurations { get; set; }
    public required DbSet<DbVersion> DbVersions { get; set; }
    public required DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
    public required DbSet<Notification> Notifications { get; set; }
    public required DbSet<Transaction> Transactions { get; set; }
    public required DbSet<TransactionMedia> TransactionMedias { get; set; }
    public required DbSet<User> Users { get; set; }
    public required DbSet<UserLoyaltyProgram> UserLoyaltyPrograms { get; set; }
    public required DbSet<UserSession> UserSessions { get; set; }
    public required DbSet<VerificationCode> VerificationCodes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");
    }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        IEnumerable<EntityEntry<BaseEntity>> entities = ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

        var now = DateTime.UtcNow;

        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                entity.Entity.CreationTime = now;
                entity.Entity.UpdateTime = now;
            }
            else
            {
                entity.Entity.UpdateTime = now;

                entity.Property(x => x.CreationTime).IsModified = false;
            }
        }
    }

}