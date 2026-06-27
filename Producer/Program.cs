using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Producer.Configuration;
using Producer.Interfaces;
using Producer.Services;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<RabbitMQConfiguration>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddScoped<IProducerService, RabbitMQProducerService>();

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

/*
warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
      Failed to determine the https port for redirect.
*/
// app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();