using Consul;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Sagas;
using OrderService.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<OrderDbContext>();
            r.UseSqlServer();
        });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost");

        cfg.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5)
            ));

        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddHostedService<OutboxWorker>();
// 1. Register the Consul Client
builder.Services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
{
    cfg.Address = new Uri("http://localhost:8500"); // Address of the Service Registry
}));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

// 2. Register the service instance on startup
app.Lifetime.ApplicationStarted.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();

    var registration = new AgentServiceRegistration()
    {
        ID = $"OrderService-{Guid.NewGuid()}", // Unique ID for this specific instance
        Name = "OrderService",                // Group name for this type of service
        Address = "localhost",                  // The instance's IP/Host
        Port = 5002,                            // The instance's dynamic port
        Check = new AgentServiceCheck()         // Health check URL for Consul to ping
        {
            HTTP = "http://localhost:5002/health",
            Interval = TimeSpan.FromSeconds(10)
        }
    };

    consulClient.Agent.ServiceRegister(registration).Wait();
});

// 3. Deregister when the service shuts down
app.Lifetime.ApplicationStopping.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    consulClient.Agent.ServiceDeregister("OrderService").Wait();
});


app.Run();
