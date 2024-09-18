namespace IncidentFinder.EventGenerator.Controllers;

using Microsoft.AspNetCore.Mvc;
using IncidentFinder.Services.EventGenerator;
using IncidentFinder.Data.Entities;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;
    private readonly EventGeneratorService _eventGeneratorService;

    public EventController(ILogger<EventController> logger, EventGeneratorService eventGeneratorService)
    {
        _logger = logger;
        _eventGeneratorService = eventGeneratorService;
    }

    [HttpPost("generate-random")]
    public async Task<IActionResult> GenerateRandomEvent()
    {
        try
        {
            await _eventGeneratorService.ExecuteAsyncByYourself();
            return Ok("Событие автоматически сгенерировано.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при автоматической генерации события.");
            return StatusCode(500, "Произошла ошибка при генерации события.");
        }
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateEvent([FromBody] EventType? eventType)
    {
        try
        {
            await _eventGeneratorService.ExecuteAsyncByYourself(eventType);
            return Ok("Событие успешно сгенерировано.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Ошибка при генерации события. Передан невалидный тип события.");
            return StatusCode(500, "Произошла ошибка при генерации события. Передан невалидный тип события.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации события.");
            return StatusCode(500, "Произошла ошибка при генерации события.");
        }
    }
}

