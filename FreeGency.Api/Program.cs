using FreeGency.AI;
using FreeGency.Api.Extensions;
using FreeGency.Api.OpenApi;
using FreeGency.Application;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Common.Results;
using FreeGency.Domain.Entities;
using FreeGency.Infrastructure;
using FreeGency.Infrastructure.Persistence.Context;
using FreeGency.Infrastructure.Persistence.Seeding;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

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
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession();
            builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("StripeSetting"));
            builder.Services.Configure<LinkedInOptions>(
    builder.Configuration.GetSection("LinkedIn"));
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
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var messages = context.ModelState
                            .Where(kvp => kvp.Value is { Errors.Count: > 0 })
                            .SelectMany(kvp => kvp.Value!.Errors.Select(err =>
                            {
                                var detail = !string.IsNullOrWhiteSpace(err.ErrorMessage)
                                    ? err.ErrorMessage
                                    : err.Exception?.Message ?? "Invalid value.";
                                return string.IsNullOrWhiteSpace(kvp.Key)
                                    ? detail
                                    : $"{kvp.Key}: {detail}";
                            }))
                            .Where(m => !string.IsNullOrWhiteSpace(m))
                            .Distinct()
                            .ToList();

                        if (messages.Count == 0)
                        {
                            messages.Add(
                                "Request binding failed for one or more form fields (check dates, numbers, skills, roadmap, or images).");
                        }

                        return new BadRequestObjectResult(
                            ApiResponse.Failure(AppError.Validation(string.Join(" | ", messages))));
                    };
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddSingleton<XmlCommentsOpenApiTransformer>();
            builder.Services.AddOpenApi(options =>
            {
                options.AddOperationTransformer<XmlCommentsOpenApiTransformer>();
                options.AddSchemaTransformer<XmlCommentsOpenApiTransformer>();
            });
            builder.Services.AddSignalR();
            builder.Services.Configure<HostOptions>(options =>
            {
                // Don't tear down the whole API if a background worker faults during shutdown/restart.
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
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
            RecurringJob.RemoveIfExists("suggestions-full-reindex");

            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var planBackground = scope.ServiceProvider.GetRequiredService<IBackGroundJobPlanService>();
            RecurringJob.AddOrUpdate("planService", () => planBackground.ProcessPlansAsync(), Cron.Daily);
            RecurringJob.AddOrUpdate("TeamPlanService", () => planBackground.ProcessPlansTeamAsync(), Cron.Daily);

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
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/hub/notifications");
            app.MapHub<ChatHub>("/hub/chat");
            app.Run();
        }
    }
}