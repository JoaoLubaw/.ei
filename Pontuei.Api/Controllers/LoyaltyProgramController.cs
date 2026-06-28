using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pontuei.Api.Commons;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Api.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Pontuei.Api.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// Global loyalty program catalogue (admin + public listing)
// ─────────────────────────────────────────────────────────────────────────────

[Route("loyalty-programs")]
[SwaggerTag("Global loyalty program catalogue")]
[Authorize]
public class LoyaltyProgramController : PontueiControllerBase
{
    private readonly ILoyaltyProgramService _loyaltyProgramService;

    public LoyaltyProgramController(
        ILoyaltyProgramService loyaltyProgramService,
        ILogger<LoyaltyProgramController> logger) : base(logger)
    {
        _loyaltyProgramService = loyaltyProgramService;
    }

    /// <summary>
    /// Returns all loyalty programs available for enrollment, ordered by name.
    /// </summary>
    /// <remarks>
    /// Only active programs are returned to non-admin callers.
    /// Used on the onboarding screen and the "Criar nova transação" program picker.
    /// </remarks>
    /// <response code="200">Returns the program list.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetLoyaltyProgramsResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpGet]
    public async Task<ActionResult<GetLoyaltyProgramsResponseDto>> GetLoyaltyPrograms([FromQuery] GetLoyaltyProgramsRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<GetLoyaltyProgramsResponseDto> apiResult = await _loyaltyProgramService.GetAllAsync(requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetLoyaltyPrograms));
        }
    }

    /// <summary>
    /// Returns the full detail of a loyalty program by ID.
    /// </summary>
    /// <response code="200">Returns the program detail.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">Program not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoyaltyProgramDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpGet("{loyaltyProgramId:int}")]
    public async Task<ActionResult<LoyaltyProgramDto>> GetLoyaltyProgramById([FromRoute] int loyaltyProgramId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<LoyaltyProgramDto> apiResult = await _loyaltyProgramService.GetByIdAsync(loyaltyProgramId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetLoyaltyProgramById));
        }
    }

    /// <summary>
    /// Creates a new loyalty program in the catalogue (admin only).
    /// </summary>
    /// <remarks>
    /// Validates name uniqueness before persisting.
    /// </remarks>
    /// <response code="201">Program created successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="409">A program with the same name already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LoyaltyProgramDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorApiResult))]
    [HttpPost]
    public async Task<ActionResult<LoyaltyProgramDto>> CreateLoyaltyProgram([FromBody] CreateLoyaltyProgramRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("CreateLoyaltyProgram called by {UserId}", currentUserId);

        try
        {
            ApiResult<LoyaltyProgramDto> apiResult = await _loyaltyProgramService.CreateAsync(requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(CreateLoyaltyProgram));
        }
    }

    /// <summary>
    /// Applies partial updates to an existing loyalty program (admin only).
    /// </summary>
    /// <response code="200">Program updated successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">Program not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoyaltyProgramDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPatch("{loyaltyProgramId:int}")]
    public async Task<ActionResult<LoyaltyProgramDto>> UpdateLoyaltyProgram(
        [FromRoute] int loyaltyProgramId,
        [FromBody] UpdateLoyaltyProgramRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("UpdateLoyaltyProgram called for program {ProgramId} by {UserId}", loyaltyProgramId, currentUserId);

        try
        {
            ApiResult<LoyaltyProgramDto> apiResult = await _loyaltyProgramService.UpdateAsync(loyaltyProgramId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(UpdateLoyaltyProgram));
        }
    }

    /// <summary>
    /// Toggles the active/inactive state of a loyalty program (admin only).
    /// </summary>
    /// <response code="200">Toggle applied successfully.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">Program not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPatch("{loyaltyProgramId:int}/toggle-active")]
    public async Task<ActionResult<bool>> ToggleActiveLoyaltyProgram([FromRoute] int loyaltyProgramId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("ToggleActiveLoyaltyProgram called for program {ProgramId} by {UserId}", loyaltyProgramId, currentUserId);

        try
        {
            ApiResult<bool> apiResult = await _loyaltyProgramService.ToggleActiveAsync(loyaltyProgramId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(ToggleActiveLoyaltyProgram));
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// User's personal enrolled program list
// ─────────────────────────────────────────────────────────────────────────────

[Route("users/{userId:guid}/loyalty-programs")]
[SwaggerTag("User loyalty program enrollment")]
[Authorize]
public class UserLoyaltyProgramController : PontueiControllerBase
{
    private readonly IUserLoyaltyProgramService _userLoyaltyProgramService;

    public UserLoyaltyProgramController(
        IUserLoyaltyProgramService userLoyaltyProgramService,
        ILogger<UserLoyaltyProgramController> logger) : base(logger)
    {
        _userLoyaltyProgramService = userLoyaltyProgramService;
    }

    /// <summary>
    /// Returns the user's enrolled loyalty programs, ordered by display order.
    /// </summary>
    /// <remarks>
    /// Each entry includes branding details for card rendering on the home-screen
    /// carousel and the program-picker screen.
    /// </remarks>
    /// <response code="200">Returns the user's program list.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserLoyaltyProgramsResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpGet]
    public async Task<ActionResult<GetUserLoyaltyProgramsResponseDto>> GetUserLoyaltyPrograms(
        [FromRoute] Guid userId,
        [FromQuery] GetUserLoyaltyProgramsRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<GetUserLoyaltyProgramsResponseDto> apiResult = await _userLoyaltyProgramService.GetByUserIdAsync(userId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetUserLoyaltyPrograms));
        }
    }

    /// <summary>
    /// Enrolls the user in a single loyalty program.
    /// </summary>
    /// <response code="201">Enrollment created successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">Loyalty program not found or inactive.</response>
    /// <response code="409">User is already enrolled in this program.</response>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserLoyaltyProgramDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorApiResult))]
    [HttpPost]
    public async Task<ActionResult<UserLoyaltyProgramDto>> EnrollInLoyaltyProgram(
        [FromRoute] Guid userId,
        [FromBody] CreateUserLoyaltyProgramRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("EnrollInLoyaltyProgram called for user {UserId}", userId);

        try
        {
            ApiResult<UserLoyaltyProgramDto> apiResult = await _userLoyaltyProgramService.EnrollAsync(userId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(EnrollInLoyaltyProgram));
        }
    }

    /// <summary>
    /// Replaces the user's full enrolled program list in a single atomic operation.
    /// </summary>
    /// <remarks>
    /// Used when the user taps "Salvar" on the card-reorder or onboarding screen.
    /// Programs not present in the new list are automatically removed.
    /// </remarks>
    /// <response code="200">Program list replaced successfully. Returns the new list.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">One or more referenced program IDs do not exist or are inactive.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserLoyaltyProgramsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPut("bulk")]
    public async Task<ActionResult<GetUserLoyaltyProgramsResponseDto>> BulkUpdateUserLoyaltyPrograms(
        [FromRoute] Guid userId,
        [FromBody] BulkUpdateUserLoyaltyProgramsRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("BulkUpdateUserLoyaltyPrograms called for user {UserId}", userId);

        try
        {
            ApiResult<GetUserLoyaltyProgramsResponseDto> apiResult = await _userLoyaltyProgramService.BulkUpdateAsync(userId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(BulkUpdateUserLoyaltyPrograms));
        }
    }

    /// <summary>
    /// Removes the user's enrollment in a single loyalty program.
    /// </summary>
    /// <response code="200">Enrollment removed successfully.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">Enrollment record not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpDelete("{loyaltyProgramId:int}")]
    public async Task<ActionResult<bool>> UnenrollFromLoyaltyProgram(
        [FromRoute] Guid userId,
        [FromRoute] int loyaltyProgramId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("UnenrollFromLoyaltyProgram called for user {UserId} and program {ProgramId}", userId, loyaltyProgramId);

        try
        {
            ApiResult<bool> apiResult = await _userLoyaltyProgramService.UnenrollAsync(userId, loyaltyProgramId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(UnenrollFromLoyaltyProgram));
        }
    }
}