using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using AuditFlow.API.Infrastructure.Auth;
using AuditFlow.API.Infrastructure.Configurations;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace AuditFlow.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));

        services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);
        services.AddFluentValidationAutoValidation();

        services.AddProblemDetails(x => x.CustomizeProblemDetails = context =>
        {
            var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
            context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        });

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Example
        // services.AddDbContext<AppDbContext>(...);
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddSingleton<ISmtpClient, SmtpClient>();

        // Change the parameters as per the requirements

        services.AddAuthenticationServices(configuration);


        return services;
    }

    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuditFlowAPI", Version = "v1" }));

        services.AddHealthChecks();

        return services;

    }

    private static IServiceCollection AddAuthenticationServices(
      this IServiceCollection services,
      IConfiguration configuration)
    {
        var jwtConfig = configuration
            .GetSection(nameof(JwtConfigurationsSettings))
            .Get<JwtConfigurationsSettings>();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(InternalRole.GetRoles)
            .Build();
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = jwtConfig.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
                };
            });

        return services;
    }
}
