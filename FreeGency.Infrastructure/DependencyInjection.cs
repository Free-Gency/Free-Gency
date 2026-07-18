
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FreeGency.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEncryptionProvider>(
                            new GenerateEncryptionProvider(
                                "713c4c4aa4f7430e973c264926219e37"));
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailService,EmailService>();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        services.AddScoped<ITeamJobRepository, TeamJobRepository>();
        services.AddScoped<ITeamJoinRequestRepository, TeamJoinRequestRepository>();
        services.AddScoped<IPortfolioProjectRepository, PortfolioProjectRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();


        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });
        services.AddOptions<EmailBinding>()
               .BindConfiguration(EmailBinding.NameSection)
               .ValidateDataAnnotations()
               .ValidateOnStart();
        return services;
    }
}
