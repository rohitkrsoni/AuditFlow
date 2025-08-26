using Serilog;

namespace AuditFlow.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomMiddleware(this WebApplication app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "OWUSportsWebAPI v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseAuthentication();

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        // Controllers wouldn't work without this
        app.MapControllers();
        app.UseAuthorization();
        app.UseSerilogRequestLogging();

        return app;
    }
}
