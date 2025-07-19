using FastTechFoods.Observability;
using FastTechFoodsAuth.Security.Extensions;
using FastTechFoodsOrder.Api.Consumers;
using FastTechFoodsOrder.Api.DI;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Api.Services;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Application.Services;
using FastTechFoodsOrder.Application.Validators;
using FastTechFoodsOrder.Infra.Repositories;
using FastTechFoodsOrder.Infra.UnitOfWork;
using FastTechFoodsOrder.Shared.Integration.Messages;
using FluentValidation;
using MongoDB.Driver;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to show in console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);



// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddScoped<FastTechFoodsOrder.Infra.Context.ApplicationDbContext>();

// Register application services
builder.Services.AddScoped<IOrderService, OrderService>();

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register Outbox services
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
builder.Services.AddHostedService<OutboxProcessorService>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateOrderSatusValidator>();

StartUpConfig.AddObservability(builder);

// Configure RabbitMQ Client .NET
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbitMqUri = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_STRING") ?? "amqp://localhost";
    return new ConnectionFactory
    {
        Uri = new Uri(rabbitMqUri),
        ClientProvidedName = "FastTechFoodsOrder.Api",
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        AutomaticRecoveryEnabled = true
    };
});

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    return factory.CreateConnectionAsync().Result;
});

builder.Services.AddScoped<IChannel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateChannelAsync().Result;
});

// Register RabbitMQ Publisher
builder.Services.AddScoped<IRabbitMQPublisher, RabbitMQPublisher>();

// Register Message Handlers
builder.Services.AddScoped<IMessageHandler<OrderAcceptedMessage>, OrderAcceptedConsumer>();
builder.Services.AddScoped<IMessageHandler<OrderPreparingMessage>, OrderPreparingConsumer>();
builder.Services.AddScoped<IMessageHandler<OrderReadyMessage>, OrderReadyConsumer>();
//builder.Services.AddScoped<IMessageHandler<OrderCompletedMessage>, OrderCompletedConsumer>();
builder.Services.AddScoped<IMessageHandler<OrderCancelledMessage>, OrderCancelledConsumer>();
//builder.Services.AddScoped<IMessageHandler<OrderCreatedMessage>, OrderCreatedConsumer>();
//builder.Services.AddScoped<IMessageHandler<OrderPendingMessage>, OrderPendingConsumer>();
//builder.Services.AddScoped<IMessageHandler<OrderStatusUpdatedMessage>, OrderStatusUpdatedConsumer>();

// Register RabbitMQ Background Service for consuming messages
builder.Services.AddHostedService<RabbitMQConsumerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFastTechFoodsSwaggerWithJwt("FastTechFoodsAuth API", "v1", "API de autenticação para o sistema FastTechFoods");
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 FastTechFoodsOrder API starting up...");

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseFastTechFoodsSecurityAudit();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowAll");

StartUpConfig.UseObservability(app);

logger.LogInformation("✅ FastTechFoodsOrder API is ready and listening for requests!");
logger.LogInformation("📊 Swagger UI available at: /swagger");
logger.LogInformation("🔍 Health checks available at: /health");

app.Run();

// Make Program class accessible for testing
public partial class Program { }

