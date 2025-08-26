using AuditFlow.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApplicationBuilder();

var app = builder.Build();

app.UseCustomMiddleware(app.Environment);
app.Run();
