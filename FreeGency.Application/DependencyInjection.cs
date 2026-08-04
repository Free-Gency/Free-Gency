

using FreeGency.Application.Features.DeveloperNotification.Commands;

namespace FreeGency.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISocialLinkService, SocialLinkService>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ProjectMapping).Assembly);
            cfg.AddMaps(typeof(ProposalMapping).Assembly);
        });
        services.AddScoped<IDeveloperNotificationService, DeveloperNotificationService>();
        services.AddSingleton<OnlineUsersService>();
        services.AddScoped<IClientNotficationService, ClientNotficationService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IExternalServices, ExternalServices>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IAuthServices, AuthServices>();
        services.AddScoped<IEmailAuthService, EmailAuthService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectFileService, ProjectFileService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<ISpecialtyService, SpecialtyService>();
        services.AddScoped<ITeamJobService, TeamJobService>();
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<ITeamService, TeamService>();

        // Proposal Ranking
        services.AddScoped<IProposalRankingService, ProposalRankingService>();
        services.AddScoped<IProposalAssistantService, ProposalAssistantService>();

        // Team Suggestions
        services.AddScoped<ITeamSuggestionService, TeamSuggestionService>();

        // Portfolio
        services.AddScoped<IPortfolioService, PortfolioService>();

        // Project
        services.AddScoped<IMilestoneService, MilestoneService>();
        services.AddScoped<IEscrowService, EscrowService>();
        services.AddScoped<IProjectEventService, ProjectEventService>();

        services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.NameSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        });

        services.AddHttpContextAccessor();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(PortfolioMappingProfile));
        });

        return services;
    }
}
