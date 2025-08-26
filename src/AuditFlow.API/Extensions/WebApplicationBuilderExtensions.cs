using Serilog;

namespace AuditFlow.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureApplicationBuilder(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "AuditFlow")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] {Level:u3} {Message:lj}{NewLine}{Exception}"));

        builder.Services
            .AddApplicationServices()
            .AddInfrastructureServices(builder.Configuration)
            .AddWebServices();

        return builder;
    }
}
