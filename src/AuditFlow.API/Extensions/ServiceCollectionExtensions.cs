using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using AuditFlow.API.Application.Behaviours;
using AuditFlow.API.Infrastructure.Auth;
using AuditFlow.API.Infrastructure.Configurations;
using AuditFlow.API.Infrastructure.Persistence;
using AuditFlow.API.Infrastructure.Persistence.Interceptors;
using AuditFlow.API.Infrastructure.Services;

using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace AuditFlow.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssemblies(typeof(Program).Assembly);
            options.AddOpenBehavior(typeof(ValidationBehaviour<,>));

        });

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

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddScoped<ICurrentUserService, DefaultCurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddScoped<AuditInterceptor>();

        services.AddMassTransit(x =>
        {
            x.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host("ap-southeast-2", _ => {});
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddDbContext<ApplicationDbContext>((serviceProvider, opts) =>
        {
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());

            if (environment.IsDevelopment())
            {
                opts.EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            }
        });

        services.AddAuthenticationServices(configuration, environment);

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

        ConfigureSwagger(services);
        services.AddHealthChecks();

        return services;

    }

    private static IServiceCollection AddAuthenticationServices(
      this IServiceCollection services,
      IConfiguration configuration,
      IWebHostEnvironment environment)
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
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = jwtConfig.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
                };
            });

        return services;
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "AuditFlowAPI", Version = "v1" });

            // Enable [SwaggerOperation] and other annotations
            swaggerGenOptions.EnableAnnotations();

            var securityScheme = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT token into field",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            swaggerGenOptions.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
            { securityScheme, Array.Empty<string>() }
            });

          var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
          var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
          swaggerGenOptions.IncludeXmlComments(xmlPath);

          swaggerGenOptions.CustomSchemaIds(s => s.Name);
        });

        //services.AddFluentValidationRulesToSwagger();
    }
}
