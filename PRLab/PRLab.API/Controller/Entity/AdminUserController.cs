using Microsoft.AspNetCore.Mvc;
using PRLab.API.DTO.User;
using PRLab.API.Mapper;
using PRLab.API.Mapper.UpdateMapper;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.UserService;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.API.Controller.Entity;

// TODO: Enable when authentication and authorization are configured.
// [Authorize(Policy = AuthorizationPolicies.Admin)]
[ApiController]
[Route("admin/users")]
public sealed class AdminUserController(
    IUserRepository userRepository,
    ISystemUserProvider systemUserProvider, 
    IAppLogger logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListUsers(
        CancellationToken ct = default)
    {
        try
        {
            var users = await userRepository.ListAsync(ct);

            return Ok(UserMapper.ToGetDTOs(users));
        }
        catch (Exception exception)
        {
            LogFailure(
                $"Failed to list users: {exception.Message}");

            return UnexpectedError();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(
        Guid id,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("User id cannot be empty.");
        }

        try
        {
            var user = await userRepository.GetByIdAsync(
                UserId.FromGuid(id),
                ct);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(UserMapper.ToGetDTO(user));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (Exception exception)
        {
            LogFailure(
                $"Failed to get user '{id}': {exception.Message}");

            return UnexpectedError();
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UserPutDTO payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("User id cannot be empty.");
        }

        try
        {
            var user = await userRepository.GetForUpdateAsync(
                UserId.FromGuid(id),
                ct);

            if (user is null)
            {
                return NotFound();
            }

            var update = UserUpdateMapper.ToUpdate(payload);

            var changedBy = systemUserProvider.GetSystemAdminUser();

            user.Update(update, changedBy);

            await userRepository.UpdateAsync(user, ct);

            return Ok(UserMapper.ToGetDTO(user));
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
            LogFailure(
                $"Failed to update user '{id}': {exception.Message}");

            return UnexpectedError();
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] UserPostDTO payload,
        CancellationToken ct = default)
    {
        try
        {
            var existingUser = await userRepository.GetByNameAsync(
                payload.Name,
                ct);

            if (existingUser is not null)
            {
                return Conflict(
                    $"A user named '{payload.Name}' already exists.");
            }

            var user = UserMapper.ToEntity(payload);

            var createdUser = await userRepository.CreateAsync(
                user,
                ct);

            return CreatedAtAction(
                nameof(GetUser),
                new { id = createdUser.Id.Value },
                UserMapper.ToGetDTO(createdUser));
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
                nameof(AdminUserController),
                $"Failed to create user: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("User id cannot be empty.");
        }

        try
        {
            var user = await userRepository.GetForUpdateAsync(
                UserId.FromGuid(id),
                ct);

            if (user is null)
            {
                return NotFound();
            }

            if (user.Audit.IsDeleted)
            {
                return NoContent();
            }

            user.MarkDeleted();

            await userRepository.UpdateAsync(user, ct);

            return NoContent();
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
            LogFailure(
                $"Failed to delete user '{id}': {exception.Message}");

            return UnexpectedError();
        }
    }

    private ObjectResult UnexpectedError()
    {
        return StatusCode(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.");
    }

    private void LogFailure(string message)
    {
        logger.Log(
            nameof(AdminUserController),
            message);
    }
}