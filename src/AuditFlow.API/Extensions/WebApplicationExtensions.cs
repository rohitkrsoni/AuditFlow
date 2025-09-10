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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuditFlowAPI v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseAuthentication();

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseAuthorization();

        // Controllers wouldn't work without this
        app.MapControllers();
        app.UseSerilogRequestLogging();

        return app;
    }
}
