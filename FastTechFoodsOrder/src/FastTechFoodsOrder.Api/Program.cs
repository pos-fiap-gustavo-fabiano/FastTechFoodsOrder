using FastTechFoodsAuth.Security.Extensions;
using FastTechFoodsOrder.Api.Consumers;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Application.Services;
using FastTechFoodsOrder.Infra.Repositories;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<FastTechFoodsOrder.Infra.Context.ApplicationDbContext>();
// Register application services
builder.Services.AddScoped<IOrderService, OrderService>();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    //x.AddConsumer<FastTechFoodsOrder.Api.Consumers.OrderStatusUpdatedConsumer>();
    //x.AddConsumer<FastTechFoodsOrder.Api.Consumers.OrderPendingConsumer>();
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
// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFastTechFoodsSwaggerWithJwt("FastTechFoodsAuth API", "v1", "API de autentica��o para o sistema FastTechFoods");
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseFastTechFoodsSecurityAudit();
app.UseAuthorization();

app.MapControllers();
app.UseCors("AllowAll");

app.Run();
