using PieShop.DataAccess;
using PieShop.Database.Initialization;

var builder = Host.CreateApplicationBuilder(args);

////builder.Services.AddDbContext<PieShopContext>();

builder.AddSqlServerDbContext<PieShopContext>("PieShop");

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddSource(Worker.ActivityName));

var host = builder.Build();

await host.RunAsync();
