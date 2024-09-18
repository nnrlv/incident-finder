namespace IncidentFinder.Services.EventProcessor;

using IncidentFinder.Data.Context;
using IncidentFinder.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EventProcessorService : BackgroundService
{
    private readonly ILogger<EventProcessorService> _logger;
    private readonly IDbContextFactory<Context> _dbContextFactory;
    private readonly TimeSpan _compositeTemplateTimeWindow = TimeSpan.FromSeconds(20);
    private static ConcurrentQueue<Event> _eventQueue = new();
    private readonly SortedList<DateTime, Event> _eventsType2 = new();

    public EventProcessorService(ILogger<EventProcessorService> logger, IDbContextFactory<Context> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task QueueEvent(Event newEvent)
    {
        _eventQueue.Enqueue(newEvent);
        _logger.LogInformation($"Событие с ID {newEvent.Id} добавлено в очередь.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_eventQueue.TryDequeue(out var newEvent))
            {
                await ProcessEvent(newEvent);
            }

            await Task.Delay(100, stoppingToken);
        }
    }

    private async Task ProcessEvent(Event newEvent)
    {
        try
        {
            if (newEvent.Type == EventType.Type2)
            {
                _eventsType2[newEvent.Time] = newEvent;
            }

            if (newEvent.Type == EventType.Type1)
            {
                var foundMatchingEvent = false;
                foreach (var oldEvent in _eventsType2.ToList())
                {
                    if ((newEvent.Time - oldEvent.Key) <= _compositeTemplateTimeWindow)
                    {
                        await CreateIncident(IncidentType.Type2, new List<Event> { newEvent, oldEvent.Value });
                        _eventsType2.Remove(oldEvent.Key);
                        foundMatchingEvent = true;
                        break;
                    }
                    else if ((newEvent.Time - oldEvent.Key) > _compositeTemplateTimeWindow)
                    {
                        _eventsType2.Remove(oldEvent.Key);
                    }
                }

                if (!foundMatchingEvent)
                {
                    await CreateIncident(IncidentType.Type1, new List<Event> { newEvent });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при обработке события с ID {newEvent.Id}.");
        }
    }

    private async Task CreateIncident(IncidentType type, List<Event> events)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Type = type,
            Time = DateTime.UtcNow,
            Events = events
        };

        context.Incidents.Add(incident);
        await context.SaveChangesAsync();

        Console.WriteLine($"Создан инцидент типа {incident.Type} Id {incident.Id} в {incident.Time}");
    }

    public async Task<IEnumerable<Incident>> GetIncidents(
    int pageNumber = 1,
    int pageSize = 10)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var incidents = await context.Incidents
            .Include(i => i.Events)
            .OrderByDescending(i => i.Time)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return incidents;
    }


}
