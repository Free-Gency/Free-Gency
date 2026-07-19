
using System.Reflection;
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectProposalRepository, ProjectProposalRepository>();
        services.AddScoped<IMilestoneRepository, MilestoneRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<IProjectFileRepository, ProjectFileRepository>();
        services.AddScoped<IProjectEventRepository, ProjectEventRepository>();
        services.AddScoped<IEscrowHoldRepository, EscrowHoldRepository>();
        services.AddScoped<ITeamPayoutSplitRepository, TeamPayoutSplitRepository>();
        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();

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
