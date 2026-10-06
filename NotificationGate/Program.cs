using NotificationGate.Services;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddConsole();
});

var programLogger = loggerFactory.CreateLogger("Program");
var kafkaLogger = loggerFactory.CreateLogger<KafkaService>();
var fileLogger = loggerFactory.CreateLogger<FileWatcherService>();


var alertsPath = @"C:\Users\user1\OneDrive\שולחן העבודה\final test\alert-simulator\alerts";

using var kafka = new KafkaService("localhost:9092",kafkaLogger);
var watcher = new FileWatcherService(alertsPath, kafka,fileLogger);
programLogger.LogInformation("ServiceStarted");


await watcher.ProcessExistingAsync();
watcher.Start();


















