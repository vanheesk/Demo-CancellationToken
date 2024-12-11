using Microsoft.AspNetCore.Mvc;

namespace CancellationDemo.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly ILogger<ProcessingController> _logger;

    public ProcessingController(ILogger<ProcessingController> logger)
    {
        _logger = logger;
    }

    [HttpGet("longrunning")]
    public async Task<IActionResult> LongRunningOperation(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting long running operation");

            // Simulate long-running process
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

            _logger.LogInformation("Long running operation completed successfully");
            return Ok(new { message = "Operation completed successfully" });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was cancelled by the client");
            return StatusCode(499, new { message = "Client closed request" });
        }
    }
}
