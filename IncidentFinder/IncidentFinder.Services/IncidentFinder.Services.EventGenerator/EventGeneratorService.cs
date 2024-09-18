namespace IncidentFinder.Services.EventGenerator;

using IncidentFinder.Data.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class EventGeneratorService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EventGeneratorService> _logger;
    private static readonly Random _random = new();

    public EventGeneratorService(IHttpClientFactory httpClientFactory, ILogger<EventGeneratorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Event GenerateEvent(EventType? eventType = null)
    {
        if (eventType.HasValue && !Enum.IsDefined(typeof(EventType), eventType.Value))
        {
            throw new ArgumentException($"Невалидный тип события: {eventType}");
        }

        return new Event
        {
            Id = Guid.NewGuid(),
            Type = eventType != null ? eventType.Value : (EventType)_random.Next(1, 5),
            Time = DateTime.UtcNow
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = _random.Next(0, 2000);
            await Task.Delay(delay, stoppingToken);

            var generatedEvent = GenerateEvent();
            _logger.LogInformation($"Событие типа {generatedEvent.Type} ID {generatedEvent.Id} сгенерировано автоматически в {DateTime.Now}");

            await SendEventToProcessor(generatedEvent);
            _logger.LogInformation($"Событие типа {generatedEvent.Type} ID {generatedEvent.Id} отправлено в {DateTime.Now}");
        }
    }

    public async Task ExecuteAsyncByYourself(EventType? eventType = null)
    {
        var generatedEvent = GenerateEvent(eventType);
        _logger.LogInformation($"Событие типа {generatedEvent.Type} ID {generatedEvent.Id} сгенерировано вручную в {DateTime.Now}");
        await SendEventToProcessor(generatedEvent);
        _logger.LogInformation($"Событие типа {generatedEvent.Type} ID {generatedEvent.Id} отправлено в {DateTime.Now}");
    }

    private async Task SendEventToProcessor(Event generatedEvent)
    {
        var client = _httpClientFactory.CreateClient();
        //var response = await client.PostAsJsonAsync("https://localhost:8080/api/events", newEvent);

        //if (!response.IsSuccessStatusCode)
        //{
        //    _logger.LogError($"Ошибка при отправке события {response.StatusCode}");
        //}
    }
}
