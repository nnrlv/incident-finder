namespace IncidentFinder.EventProcessor.Controllers;

using IncidentFinder.Data.Entities;
using IncidentFinder.Services.EventProcessor;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class IncidentController : ControllerBase
{
    private readonly ILogger<IncidentController> _logger;
    private readonly EventProcessorService _eventProcessorService;

    public IncidentController(ILogger<IncidentController> logger, EventProcessorService eventProcessorService)
    {
        _logger = logger;
        _eventProcessorService = eventProcessorService;
    }

    [HttpGet("")]
    public async Task<IEnumerable<Incident>> GetIncidents([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        return await _eventProcessorService.GetIncidents(pageNumber, pageSize);
    }

    [HttpPost("")]
    public async Task<IActionResult> ProcessEvent([FromBody] Event newEvent)
    {
        await _eventProcessorService.QueueEvent(newEvent);
        return Ok();
    }
}

