using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CommandWorker.Services;

public class ImportantLogService
{
    private readonly HttpClient _http = new();
    private readonly ILogger<ImportantLogService> _logger;
    private const string ElasticUrl = "http://localhost:9200/kolaman-logs/_doc";

    public ImportantLogService(ILogger<ImportantLogService> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(
        string eventType,
        string? alertId = null,
        string? command = null,
        string? message = null)
    {
        var document = new
        {
            service = "CommandWorker",
            eventType,
            alertId,
            command,
            message,
            timestamp = DateTime.UtcNow
        };

        try
        {
            var response = await _http.PostAsJsonAsync(ElasticUrl, document);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,"Could not write important log to Elasticsearch");
        }
    }
}
