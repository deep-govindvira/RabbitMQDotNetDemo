```

mkdir RabbitMQDotNetDemo
cd RabbitMQDotNetDemo
dotnet new sln -n RabbitMQDotNetDemo
dotnet new webapi -n Producer
dotnet new webapi -n Consumer
dotnet sln add Producer
dotnet sln add Consumer
cd Producer
dotnet add package RabbitMQ.Client
cd ..
cd Consumer
dotnet add package RabbitMQ.Client
cd ..
cd Producer
dotnet list package
dotnet add package AspNetCore.HealthChecks.Rabbitmq
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Settings.Configuration

```