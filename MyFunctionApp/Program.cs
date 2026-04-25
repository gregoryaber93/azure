using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyFunctionApp.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // ✅ Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();

        // 🔥 KLUCZOWE — logger pipeline
        services.Configure<TelemetryConfiguration>(telemetryConfig =>
        {
            telemetryConfig.ConnectionString =
                config["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        });

        // DB
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
    })
    .Build();
 
host.Run();