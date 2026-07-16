using FreeGency.Infrastructure.Persistence.Schemas;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Extensions;

public static class IdentityModelBuilderExtensions
{
    public static void ApplyIdentitySchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole<Guid>>(b => b.ToTable("Roles", DbSchemas.Identity));
        modelBuilder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("UserRoles", DbSchemas.Identity));
        modelBuilder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims", DbSchemas.Identity));
        modelBuilder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins", DbSchemas.Identity));
        modelBuilder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens", DbSchemas.Identity));
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims", DbSchemas.Identity));
    }
}
