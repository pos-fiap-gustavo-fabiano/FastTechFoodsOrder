using FastTechFoods.Observability;
using FastTechFoodsAuth.Security.Extensions;
using FastTechFoodsOrder.Api.Consumers;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Application.Services;
using FastTechFoodsOrder.Application.Validators;
using FastTechFoodsOrder.Infra.Repositories;
using FastTechFoodsOrder.Infra.UnitOfWork;
using FluentValidation;
using MassTransit;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE") 
    ?? "mongodb://localhost:27017";

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(connectionString);
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register DbContext
builder.Services.AddScoped<FastTechFoodsOrder.Infra.Context.ApplicationDbContext>();

// Register application services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderMessagePublisher, OrderMessagePublisher>();

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register Outbox services
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
builder.Services.AddHostedService<OutboxProcessorService>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateOrderSatusValidator>();

// Observability & Metrics
builder.Services.AddFastTechFoodsObservabilityWithSerilog(builder.Configuration);
builder.Services.AddFastTechFoodsPrometheus(builder.Configuration);
// Adicionar ActivitySources customizados para melhor detalhamento no Jaeger
builder.Services.Configure<OpenTelemetry.Trace.TracerProviderBuilder>(tracerBuilder =>
{
    tracerBuilder
        .AddSource("FastTechFoodsOrder.OrderService")      // Traces do OrderService
        .AddSource("FastTechFoodsOrder.OutboxProcessor");   // Traces do OutboxProcessor
});

builder.Services.AddFastTechFoodsHealthChecksWithMongoDB(builder.Configuration, connectionString);

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FastTechFoodsOrder.Api.Consumers.OrderAcceptedConsumer>();
    x.AddConsumer<FastTechFoodsOrder.Api.Consumers.OrderPreparingConsumer>();
    x.AddConsumer<FastTechFoodsOrder.Api.Consumers.OrderReadyConsumer>();
    x.AddConsumer<FastTechFoodsOrder.Api.Consumers.OrderCompletedConsumer>();
    x.AddConsumer<FastTechFoodsOrder.Api.Consumers.OrderCancelledConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_STRING") ?? "rabbitmq://localhost";
        cfg.Host(rabbitMqUri);

        cfg.ReceiveEndpoint("order.accepted.queue", e =>
        {
            e.ConfigureConsumer<OrderAcceptedConsumer>(context);
        });

        cfg.ReceiveEndpoint("order.preparing.queue", e =>
        {
            e.ConfigureConsumer<OrderPreparingConsumer>(context);
        });

        cfg.ReceiveEndpoint("order.ready.queue", e =>
        {
            e.ConfigureConsumer<OrderReadyConsumer>(context);
        });

        cfg.ReceiveEndpoint("order.completed.queue", e =>
        {
            e.ConfigureConsumer<OrderCompletedConsumer>(context);
        });

        cfg.ReceiveEndpoint("order.cancelled.queue", e =>
        {
            e.ConfigureConsumer<OrderCancelledConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

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

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseFastTechFoodsSecurityAudit();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowAll");
app.UseFastTechFoodsHealthChecksUI();
app.UseFastTechFoodsPrometheus();

app.Run();
