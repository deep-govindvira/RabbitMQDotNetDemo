using Consumer.Configuration;
using Serilog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using Consumer.Services;
using Consumer.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext();
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<RabbitMQConfiguration>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<RabbitMQConsumerService>();
builder.Services.AddSingleton<IOrderService, OrderService>();

builder.Services
    .AddHealthChecks()
    .AddRabbitMQ(serviceProvider =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;

        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };

        return factory.CreateConnectionAsync();
    },
    name: "rabbitmq")
    .AddCheck("self", () => HealthCheckResult.Healthy());

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();
