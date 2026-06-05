using MassTransit;
using NotificationService.Worker.Consumers;
using NotificationService.Worker.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<NotificationStore>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost");

        cfg.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5)
            ));

        cfg.ReceiveEndpoint("notification-queue", e =>
        {
            e.ConfigureConsumer<NotificationConsumer>(ctx);
            e.DeadLetterExchange = "dead-letter-exchange";
        });
    });
});

var app = builder.Build();

app.MapGet("/api/notifications", (NotificationStore store) =>
    store.GetAll());

app.Run();
