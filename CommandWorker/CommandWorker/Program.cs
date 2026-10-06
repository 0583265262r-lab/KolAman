using CommandWorker.Data;
using CommandWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

var connectionString =
    "server=localhost;" +
    "port=3306;" +
    "database=kolaman;" +
    "user=root;" +
    "password=1234";

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        )
);

builder.Services.AddSingleton<ImportantLogService>();
builder.Services.AddHostedService<RabbitConsumer>();

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Program");
var importantLogs = host.Services.GetRequiredService<ImportantLogService>();

try
{
    logger.LogInformation("Creating/checking MySQL database");

    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    logger.LogInformation("MySQL database and command tables are ready");
    await importantLogs.WriteAsync(
        "ServiceStarted",
        message: "CommandWorker started and MySQL is ready"
    );
}
catch (Exception ex)
{
    logger.LogError(ex, "Could not initialize MySQL");
    await importantLogs.WriteAsync(
        "DatabaseStartupError",
        message: ex.Message
    );
    throw;
}

await host.RunAsync();

