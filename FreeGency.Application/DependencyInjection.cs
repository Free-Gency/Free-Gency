using FluentValidation.AspNetCore;
using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Mappings.ProjectMappings;
using FreeGency.Application.Features.Account.Queries;
using FreeGency.Application.Features.Authentication;
using FreeGency.Application.Features.categories.Commands;
using FreeGency.Application.Features.EmailFeature.Commands;
using FreeGency.Application.Features.ExternalFeature.Commands;
using FreeGency.Application.Features.Projects.Commands;
using FreeGency.Application.Features.skills.Commands;
using FreeGency.Application.Features.specialties.Commands;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FreeGency.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(ProjectMapping).Assembly);
            });
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IExternalServices, ExternalServices>();
            services.AddSingleton<IJwtProvider, JwtProvider>();
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<IEmailAuthService, EmailAuthService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<ISpecialtyService, SpecialtyService>();
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

            return services;
        }
    }
}
