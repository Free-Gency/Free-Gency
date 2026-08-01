using FluentValidation.AspNetCore;
using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Mappings.PortfolioMappings;
using FreeGency.Application.Common.Mappings.ProjectMappings;
using FreeGency.Application.Common.Mappings.ProposalsMapping;
using FreeGency.Application.Features.Account.Queries;
using FreeGency.Application.Features.Authentication;
using FreeGency.Application.Features.categories.Commands;
using FreeGency.Application.Features.EmailFeature.Commands;
using FreeGency.Application.Features.Escrow.Commands;
using FreeGency.Application.Features.ExternalFeature.Commands;
using FreeGency.Application.Features.Milestones.Commands;
using FreeGency.Application.Features.Portfolio.Commands;
using FreeGency.Application.Features.ProjectEvents.Commands;
using FreeGency.Application.Features.ProjectFiles.Commands;
using FreeGency.Application.Features.Projects.Commands;
using FreeGency.Application.Features.ProposalAssistant;
using FreeGency.Application.Features.ProposalRanking;
using FreeGency.Application.Features.Proposals.Commands;
using FreeGency.Application.Features.skills.Commands;
using FreeGency.Application.Features.SocialLinks.Commands;
using FreeGency.Application.Features.specialties.Commands;
using FreeGency.Application.Features.TeamJobs.Commands;
using FreeGency.Application.Features.Teams.Commands;
using FreeGency.Application.Features.WalletFeature.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FreeGency.Application.Features.SocialLinks.Commands;
using FreeGency.Application.Features.Proposals.Commands;
using FreeGency.Application.Common.Mappings.ProposalsMapping;
using FreeGency.Application.Features.Teams.Commands;
using FreeGency.Application.Features.ProposalRanking;
using FreeGency.Application.Features.Portfolio.Commands;
using FreeGency.Application.Features.WalletFeature.Queries;
using FreeGency.Application.Features.ClientNotification.Commands;
using FreeGency.Application.Features.ChatFeature.Commands;

namespace FreeGency.Application
{
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
}
