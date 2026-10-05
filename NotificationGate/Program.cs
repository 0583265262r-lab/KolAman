using NotificationGate.Services;
var alertsPath = @"C:\Users\user1\OneDrive\שולחן העבודה\final test\alert-simulator\alerts";

using var kafka = new KafkaService("localhost:9092");

var watcher = new FileWatcherService(alertsPath, kafka);

await watcher.ProcessExistingAsync();
watcher.Start();