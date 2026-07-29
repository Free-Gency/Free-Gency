using CloudinaryDotNet;
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Implementations;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using FreeGency.Infrastructure.Interfaces;
using FreeGency.Infrastructure.Persistence.Context;
using FreeGency.Infrastructure.Persistence.Interceptors;
using FreeGency.Infrastructure.Persistence.Repositories;
using FreeGency.Infrastructure.Persistence.Repositories.Reviews;
using FreeGency.Domain.Interfaces.Repositories.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CloudinaryClient = CloudinaryDotNet.Cloudinary;

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
        services.AddSingleton<CloudinaryClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;
            return new CloudinaryClient(new Account(options.CloudName, options.ApiKey, options.ApiSecret));
        });
        services.AddScoped<IClientNotificationSettingsRepository, ClientNotificationSettingsRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<ISocialLinkRepository, SocialLinkRepository>();
        services.AddScoped<IStorageService, CloudinaryStorageService>();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        services.AddScoped<ITeamJobRepository, TeamJobRepository>();
        services.AddScoped<ITeamJoinRequestRepository, TeamJoinRequestRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectProposalRepository, ProjectProposalRepository>();
        services.AddScoped<IMilestoneRepository, MilestoneRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<IProjectFileRepository, ProjectFileRepository>();
        services.AddScoped<IProjectEventRepository, ProjectEventRepository>();
        services.AddScoped<IEscrowHoldRepository, EscrowHoldRepository>();
        services.AddScoped<ITeamPayoutSplitRepository, TeamPayoutSplitRepository>();
        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
        services.AddScoped<IClientProfileRepository, ClientProfileRepository>();
        services.AddScoped<IDeveloperProfileRepository, DeveloperProfileRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ILedgerEntryRepository, LedgerEntryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();


        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseLazyLoadingProxies();
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });
        services.AddOptions<EmailBinding>()
               .BindConfiguration(EmailBinding.NameSection)
               .ValidateDataAnnotations()
               .ValidateOnStart();
        services.AddOptions<CloudinaryOptions>()
               .BindConfiguration(CloudinaryOptions.NameSection)
               .ValidateDataAnnotations()
               .ValidateOnStart();
        return services;
    }
}