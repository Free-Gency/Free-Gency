using FreeGency.AI;
using FreeGency.Application;
using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Hubs;
using FreeGency.Domain.Entities;
using FreeGency.Infrastructure;
using FreeGency.Infrastructure.Persistence.Context;
using FreeGency.Infrastructure.Persistence.Seeding;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace FreeGency.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
            builder.Services.AddInfrastructure(builder.Configuration)
                            .AddApplication();
            builder.Services.AddAI(builder.Configuration);
            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("StripeSetting"));
            #region
            var JwtSettings = builder.Configuration.GetSection(JwtOptions.NameSection).Get<JwtOptions>();
            builder.Host.UseSerilog((context, configration) =>
                    configration.ReadFrom.Configuration(context.Configuration));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
                 ).AddJwtBearer(o =>
                 {
                     o.SaveToken = true;
                     o.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuerSigningKey = true,
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings?.Key!)),
                         ValidIssuer = JwtSettings?.Issuer,
                         ValidAudience = JwtSettings?.Audience
                     };
                     o.Events = new JwtBearerEvents
                     {
                         OnMessageReceived = context =>
                         {
                             var accessToken = context.Request.Query["access_token"];

                             var path = context.HttpContext.Request.Path;

                             if (!string.IsNullOrEmpty(accessToken) &&
                                 path.StartsWithSegments("/hub"))
                             {
                                 context.Token = accessToken;
                             }

                             return Task.CompletedTask;
                         }
                     };
                 }).AddCookie().AddGoogle(options =>
                 {
                     options.ClientId =
                         builder.Configuration["Authentication:Google:ClientId"]!;

                     options.ClientSecret =
                         builder.Configuration["Authentication:Google:ClientSecret"]!;
                     options.SignInScheme = IdentityConstants.ExternalScheme;
                 });

            #endregion
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSignalR();
            builder.Services.AddHostedService<FreeGency.Api.BackgroundJobs.MilestoneAutoReleaseWorker>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    var origins = builder.Configuration
                        .GetSection("FrontendUrls")
                        .Get<string[]>() ?? [builder.Configuration["FrontendUrl"] ?? "http://localhost:4200"];

                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            builder.Services.AddHangfire(configration => configration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
            builder.Services.AddHangfireServer();
            var app = builder.Build();
            app.UseStaticFiles();
            app.UseHangfireDashboard("/jobs");
            DatabaseInitializer.InitializeAsync(app.Services).GetAwaiter().GetResult();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseHttpsRedirection();
            }
            else
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }
            app.UseSerilogRequestLogging();
            app.UseCors("Frontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/hub/notifications");
            app.MapHub<ChatHub>("/hub/chat");
            app.Run();
        }
    }
}
