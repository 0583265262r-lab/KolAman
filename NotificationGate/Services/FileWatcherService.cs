using Microsoft.Extensions.Logging;

namespace NotificationGate.Services;

public class FileWatcherService
{
    private readonly string _alertsPath;
    private readonly KafkaService _kafka;
    private readonly ILogger<FileWatcherService> _logger;


    public FileWatcherService(string alertsPath, 
                              KafkaService kafka,
                              ILogger<FileWatcherService> logger)
    {
        _logger = logger;
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

        _logger.LogInformation($"Watching: {_alertsPath}");
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
                _logger.LogWarning($"alert.json not found: {folder}");
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            await _kafka.SendAsync(json);
            _logger.LogInformation($"Sent to Kafka: {folder}");

            

            Directory.Delete(folder, recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"NotificationGate error: {ex.Message}");
            Console.WriteLine($"NotificationGate error: {ex.Message}");
        }
    }
}
