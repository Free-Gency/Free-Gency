using FreeGency.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace FreeGency.Infrastructure.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyAuditableConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType, builder =>
            {
                builder.Property(nameof(IAuditableEntity.CreatedAt))
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETUTCDATE()");

                builder.Property(nameof(IAuditableEntity.CreatedBy))
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasDefaultValue("system");

                builder.Property(nameof(IAuditableEntity.UpdatedAt))
                    .HasColumnType("datetime2");

                builder.Property(nameof(IAuditableEntity.UpdatedBy))
                    .HasMaxLength(256);
            });
        }
    }

    public static void ApplySoftDeleteConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType, builder =>
            {
                builder.Property(nameof(ISoftDeletableEntity.IsDeleted))
                    .IsRequired()
                    .HasDefaultValue(false);

                builder.Property(nameof(ISoftDeletableEntity.DeletedAt))
                    .HasColumnType("datetime2");

                builder.Property(nameof(ISoftDeletableEntity.DeletedBy))
                    .HasMaxLength(256);
            });

            SetSoftDeleteQueryFilter(modelBuilder, entityType.ClrType);
        }
    }

    private static void SetSoftDeleteQueryFilter(ModelBuilder modelBuilder, Type entityType)
    {
        var method = typeof(ModelBuilderExtensions)
            .GetMethod(nameof(SetSoftDeleteQueryFilterGeneric), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityType);

        method.Invoke(null, [modelBuilder]);
    }

    private static void SetSoftDeleteQueryFilterGeneric<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletableEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
