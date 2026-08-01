using Microsoft.AspNetCore.Mvc;
using PRLab.API.DTO.User;
using PRLab.API.Mapper;
using PRLab.API.Mapper.UpdateMapper;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.UserService;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.API.Controller.Entity;

[ApiController]
[Route("users")]
public sealed class UserController(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IAppLogger logger)
    : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(
        CancellationToken ct = default)
    {
        try
        {
            var user = await currentUserService
                .GetRequiredCurrentUserAsync(ct);

            return Ok(UserMapper.ToGetDTO(user));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(UserController),
                $"Failed to get current user: {exception.Message}");

            return UnexpectedError();
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser(
        [FromBody] CurrentUserPutDTO payload,
        CancellationToken ct = default)
    {
        try
        {
            var currentUserId =
                currentUserService.GetRequiredUserId();

            var user = await userRepository.GetForUpdateAsync(
                currentUserId,
                ct);

            if (user is null)
            {
                return NotFound();
            }

            var update =
                UserUpdateMapper.ToUpdate(payload);

            user.Update(
                update,
                changedBy: user);

            await userRepository.UpdateAsync(
                user,
                ct);

            return Ok(UserMapper.ToGetDTO(user));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(UserController),
                $"Failed to update current user: {exception.Message}");

            return UnexpectedError();
        }
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrentUser(
        CancellationToken ct = default)
    {
        try
        {
            var userId = currentUserService.GetRequiredUserId();

            var user = await userRepository.GetForUpdateAsync(
                userId,
                ct);

            if (user is null)
            {
                return NotFound();
            }

            user.MarkDeleted();

            await userRepository.UpdateAsync(user, ct);

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(UserController),
                $"Failed to delete current user: {exception.Message}");

            return UnexpectedError();
        }
    }

    private ObjectResult UnexpectedError()
    {
        return StatusCode(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.");
    }
}