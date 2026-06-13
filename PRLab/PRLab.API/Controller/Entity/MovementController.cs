using Microsoft.AspNetCore.Mvc;
using PRLab.API.DTO.Movement;
using PRLab.API.Mapper;
using PRLab.API.Mapper.UpdateMapper;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Controller.Entity;

[ApiController]
[Route("movements")]
public sealed class MovementController : ControllerBase
{
    private readonly IMovementRepository repo;
    private readonly IAppLogger logger;
    private readonly IUserService userService;

    public MovementController(
        IMovementRepository repo,
        IUserService userService,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
        this.userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMovement(
        Guid id,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Movement id cannot be empty.");
        }

        try
        {
            var movement = await repo.GetByIdAsync(MovementId.FromGuid(id), ct);

            if (movement is null)
            {
                return NotFound();
            }

            return Ok(MovementMapper.ToGetDTO(movement, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementController),
                $"Failed to get movement {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMovements(
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        try
        {
            var movements = await repo.ListAsync(ct);

            return Ok(MovementMapper.ToGetDTOs(movements, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementController),
                $"Failed to get movements: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovement(
        MovementPostDTO payload,
        CancellationToken ct = default)
    {
        try
        {
            var nameExists = await repo.NameExistsAsync(
                payload.Name,
                null,
                ct);

            if (nameExists)
            {
                return Conflict("A movement with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            var movement = MovementMapper.ToEntity(payload, activeUser);

            var createdMovement = await repo.CreateAsync(movement, ct);

            return CreatedAtAction(
                nameof(GetMovement),
                new { id = createdMovement.Id.Value },
                MovementMapper.ToGetDTO(createdMovement));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementController),
                $"Failed to create movement: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMovement(
        Guid id,
        MovementPutDTO payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Movement id cannot be empty.");
        }

        try
        {
            var movementId = MovementId.FromGuid(id);

            var movement = await repo.GetTrackedByIdAsync(movementId, ct);

            if (movement is null)
            {
                return NotFound();
            }

            var nameExists = await repo.NameExistsAsync(
                payload.Name,
                movementId,
                ct);

            if (nameExists)
            {
                return Conflict("Another movement with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);
            var update = MovementUpdateMapper.ToUpdate(movement, payload, activeUser);

            movement.Update(update);

            await repo.UpdateAsync(movement, ct);

            return Ok(MovementMapper.ToGetDTO(movement));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementController),
                $"Failed to update movement {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }
}