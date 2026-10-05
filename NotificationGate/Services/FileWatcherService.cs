using Microsoft.Extensions.Logging;

namespace NotificationGate.Services;

public class FileWatcherService
{
    private readonly string _alertsPath;
    private readonly KafkaService _kafka;

    public FileWatcherService(string alertsPath, 
                              KafkaService kafka)
    {
        _alertsPath = alertsPath;
        _kafka = kafka;
    }

    public async Task ProcessExistingAsync()
    {
        if (!Directory.Exists(_alertsPath))
            return;

        foreach (var ready in Directory.GetFiles(
                     _alertsPath,
                     "alert.ready",
                     SearchOption.AllDirectories))
        {
            await ProcessReadyAsync(ready);
        }
    }

    public void Start()
    {

        using var watcher = new FileSystemWatcher(_alertsPath)
        {
            Filter = "alert.ready",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
            InternalBufferSize = 65536,
            EnableRaisingEvents = true
        };

        watcher.Created += async (_, e) =>
        {
            await ProcessReadyAsync(e.FullPath);
        };

        watcher.Error += (_, e) =>
        {
            Console.WriteLine($"FileSystemWatcher error: {e.GetException().Message}");
        };

        Console.WriteLine($"Watching: {_alertsPath}");
        Console.WriteLine("Press Enter to stop.");
        Console.ReadLine();
    }

    private async Task ProcessReadyAsync(string readyPath)
    {
        try
        {
            var folder = Path.GetDirectoryName(readyPath);
            if (folder == null)
                return;

            var jsonPath = Path.Combine(folder, "alert.json");

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"alert.json not found: {folder}");
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            await _kafka.SendAsync(json);

            Console.WriteLine($"Sent to Kafka: {folder}");

            Directory.Delete(folder, recursive: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationGate error: {ex.Message}");
        }
    }
}
