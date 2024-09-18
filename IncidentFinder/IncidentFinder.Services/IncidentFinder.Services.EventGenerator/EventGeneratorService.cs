namespace IncidentFinder.Services.EventGenerator;

using IncidentFinder.Data.Entities;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

public class EventGeneratorService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EventGeneratorService> _logger;
    private static readonly Random _random = new();
    private Event? _event = null;

    public EventGeneratorService(IHttpClientFactory httpClientFactory, ILogger<EventGeneratorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Event GenerateEvent(EventType? eventType = null)
    {
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

            _event = GenerateEvent();

            await SendEventToProcessor(_event);

            _logger.LogInformation($"Событие с ID {_event.Id} отправлено в {DateTime.Now}");
        }
    }

    public async Task ExecuteAsyncByYourself(EventType? eventType = null)
    {
        var newEvent = GenerateEvent(eventType);
        await SendEventToProcessor(newEvent);
        _logger.LogInformation($"Событие с ID {newEvent.Id} отправлено в {DateTime.Now}");
    }

    private async Task SendEventToProcessor(Event newEvent)
    {
        var client = _httpClientFactory.CreateClient();
        //var response = await client.PostAsJsonAsync("https://localhost:8080/api/events", newEvent);

        //if (!response.IsSuccessStatusCode)
        //{
        //    _logger.LogError($"Ошибка при отправке события {response.StatusCode}");
        //}
    }
}
