using Serilog;

namespace AuditFlow.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomMiddleware(this WebApplication app, IHostEnvironment env)
    {
        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuditFlowAPI v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStatusCodePages();

        // Controllers wouldn't work without this
        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
