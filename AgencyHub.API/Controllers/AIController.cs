using AgencyHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgencyHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ai")]
public sealed class AIController : ControllerBase
{
    private readonly AIChatService _aiChatService;
    private readonly ILogger<AIController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AIController(
        AIChatService aiChatService,
        ILogger<AIController> logger,
        IWebHostEnvironment environment)
    {
        _aiChatService = aiChatService;
        _logger = logger;
        _environment = environment;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] AIChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new
            {
                message = "Please enter a message."
            });
        }

        if (request.Message.Length > 4000)
        {
            return BadRequest(new
            {
                message = "Message cannot exceed 4000 characters."
            });
        }

        try
        {
            var response = await _aiChatService.AskAsync(
                request.Message.Trim(),
                cancellationToken);

            return Ok(new
            {
                message = response
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new
            {
                message = "Request was cancelled."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "AI chat request failed.");

            if (_environment.IsDevelopment())
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }

            return StatusCode(500, new
            {
                message =
                    "The AI assistant is temporarily unavailable."
            });
        }
    }
}

public sealed class AIChatRequest
{
    public string Message { get; set; } = string.Empty;
}