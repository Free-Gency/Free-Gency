using FluentValidation;
using FluentValidation.AspNetCore;
using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace FreeGency.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IJwtProvider, JwtProvider>();
            services.AddFluentValidationAutoValidation()
                    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.NameSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                //options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            });

            return services;
        }
    }
}
