using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pontuei.Api.Commons;
using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Pontuei.Api.Controllers;

[Route("users")]
[SwaggerTag("User account management")]
[Authorize]
public class UserController : PontueiControllerBase
{
    private readonly IUserService _userService;

    public UserController(
        IUserService userService,
        ILogger<UserController> logger) : base(logger)
    {
        _userService = userService;
    }

    /// <summary>
    /// Returns a paginated list of users matching the given filter criteria.
    /// </summary>
    /// <response code="200">Returns the list of users.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUsersResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpGet]
    public async Task<ActionResult<GetUsersResponseDto>> GetUsers([FromQuery] GetUsersRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("GetUsers called by {UserId}", currentUserId);

        try
        {
            ApiResult<GetUsersResponseDto> apiResult = await _userService.GetUsers(requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetUsers));
        }
    }

    /// <summary>
    /// Returns a user summary by ID.
    /// </summary>
    /// <response code="200">Returns the user summary.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">User not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpGet("{userId:guid}")]
    [EnableRateLimiting("StrictAuthLimit")]
    public async Task<ActionResult<UserDto>> GetUserById([FromRoute] Guid userId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<UserDto> apiResult = await _userService.GetByIdAsync(userId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetUserById));
        }
    }

    /// <summary>
    /// Applies partial updates to a user account from the account settings screen.
    /// </summary>
    /// <remarks>
    /// When the e-mail changes, a new verification flow is triggered automatically.
    /// </remarks>
    /// <response code="200">User updated successfully. Returns the updated summary.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">User not found.</response>
    /// <response code="409">The new e-mail is already taken by another account.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorApiResult))]
    [HttpPatch("{userId:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("UpdateUser called for {UserId} by {CurrentUserId}", userId, currentUserId);

        try
        {
            ApiResult<UserDto> apiResult = await _userService.UpdateAsync(userId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(UpdateUser));
        }
    }

    /// <summary>
    /// Soft-deletes the user account and revokes all active sessions.
    /// </summary>
    /// <response code="204">Account deleted successfully.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">User not found.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult> DeleteAccount([FromRoute] Guid userId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("DeleteAccount called for {UserId} by {CurrentUserId}", userId, currentUserId);

        try
        {
            ApiResult<EmptyDto> apiResult = await _userService.DeleteAccountAsync(userId, currentUserId.Value);
            return ToNoContentResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(DeleteAccount));
        }
    }
}