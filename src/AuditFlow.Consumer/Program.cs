using AuditFlow.Consumer.Consumers;
using AuditFlow.Consumer.Persistence;
using AuditFlow.Consumer.Validators;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

// setup Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

services.AddValidatorsFromAssemblyContaining<AuditTransactionMessageValidator>();

services.AddScoped<IAuditDbContext, AuditDbContext>();

services.AddMassTransit(x =>
{

    x.AddConsumer<AuditTransactionConsumer>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host("ap-southeast-2", _ => { });
        cfg.ConfigureEndpoints(context);
    });
});

services.AddDbContext<AuditDbContext>((serviceProvider, opts) =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
    {
        opts.EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }
});


var host = builder.Build();
host.Run();
