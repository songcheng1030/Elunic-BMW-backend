using System;
using System.Threading;
using System.Threading.Tasks;
using AIQXCommon.Models;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AIQXCoreService.Implementation.Persistence
{
    public class AppDbContext : DbContext
    {

        public DbSet<PlantEntity> Plants { get; set; }
        public DbSet<UseCaseEntity> UseCases { get; set; }
        public DbSet<AttachmentEntity> Attachments { get; set; }
        public DbSet<UseCaseStepEntity> UseCaseSteps { get; set; }
        public DbSet<UseCaseStepContentEntity> UseCaseStepContent { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> ctx) : base(ctx)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(ConfigService.GetInstance().TablePrefix() + entity.GetTableName().ToLower());
            }

            modelBuilder.Entity<PlantEntity>()
                .HasIndex(u => u.Name)
                .IsUnique();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return (await base.SaveChangesAsync(cancellationToken));
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            var utcNow = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.Entity is UpdatedAtModel trackable)
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedAt = utcNow;
                            entry.Property("UpdatedAt").IsModified = true;
                            break;

                        case EntityState.Added:
                            trackable.UpdatedAt = utcNow;
                            break;
                    }
                }
            }
        }
    }
}