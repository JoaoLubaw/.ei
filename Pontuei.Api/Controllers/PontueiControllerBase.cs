using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Pontuei.Api.Commons;
using Pontuei.Shared.Dtos.Objects;

namespace Pontuei.Api.Controllers;

[ApiController]
public abstract class PontueiControllerBase : ControllerBase
{
    protected readonly ILogger _logger;

    protected PontueiControllerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected Guid? CurrentUserId()
    {
        string? raw = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                   ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        return Guid.TryParse(raw, out Guid id) ? id : null;
    }

    protected string? CurrentUserName() =>
        User.Claims.FirstOrDefault(c => c.Type == "name")?.Value
        ?? User.Identity?.Name;

    // ── ApiResult → ActionResult conversion ───────────────────────────────

    /// <summary>
    /// Converts an ApiResult to an ActionResult, mapping the internal result code to an appropriate HTTP status code and response body.
    /// </summary>
    protected ActionResult ToActionResult<T>(ApiResult<T> result)
    {
        return result.HttpCode switch
        {
            HttpStatusCode.OK => Ok(result.Data),
            HttpStatusCode.Created => Created(string.Empty, result.Data),
            HttpStatusCode.Accepted => Accepted(result.Data),
            HttpStatusCode.NoContent => NoContent(),
            _ => ErrorResponse(result.ResultCode, result.HttpCode)
        };
    }

    /// <summary>
    /// Version for responses without a body (EmptyDto), which returns 204 NoContent on success.
    /// </summary>
    protected ActionResult ToNoContentResult(ApiResult<EmptyDto> result)
    {
        if (result.HttpCode == HttpStatusCode.OK || result.HttpCode == HttpStatusCode.NoContent)
            return NoContent();

        return ErrorResponse(result.ResultCode, result.HttpCode);
    }

    // ── Error message ──────────────────────────────────────

    private ObjectResult ErrorResponse(InternalResultCode code, HttpStatusCode httpCode)
    {
        ErrorApiResult body = new ErrorApiResult(
            code,
            (int)httpCode,
            ErrorMessages.Get(code)
        );

        return StatusCode((int)httpCode, body);
    }

    // ── Treatment of unhandled exceptions ─────────────────────────────

    protected ActionResult HandleException(Exception ex, string? context = null)
    {
        _logger.LogError(ex, "Unhandled exception in {Context}: {Message}", context ?? "controller", ex.Message);

        InternalResultCode code = ex switch
        {
            TimeoutException => InternalResultCode.TIMEOUT,
            UnauthorizedAccessException => InternalResultCode.UNLOGGED,
            NotImplementedException => InternalResultCode.SERVICE_UNAVAILABLE,
            TaskCanceledException => InternalResultCode.SERVICE_UNAVAILABLE,
            _ => InternalResultCode.UNHANDLED_ERROR
        };

        return ErrorResponse(code, HttpStatusCode.InternalServerError);
    }
}