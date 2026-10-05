using NotificationGate.Services;
using Elastic.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();


builder.Logging.AddElasticsearch();


var host = builder.Build();

await host.StartAsync();

var logger =
    host.Services.GetRequiredService<ILogger<Program>>();

var alertsPath = @"C:\Users\user1\OneDrive\שולחן העבודה\final test\alert-simulator\alerts";

logger.LogInformation("Application started");
using var kafka = new KafkaService("localhost:9092");
logger.LogInformation("generate kafka");
var watcher = new FileWatcherService(alertsPath, kafka);



await watcher.ProcessExistingAsync();
watcher.Start();

await host.StopAsync();

















