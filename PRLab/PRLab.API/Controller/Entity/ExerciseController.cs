using Microsoft.AspNetCore.Mvc;
using PRLab.API.DTO.Exercise;
using PRLab.API.Mapper;
using PRLab.API.Mapper.UpdateMapper;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.API.Controller.Entity;

[ApiController]
[Route("exercises")]
public sealed class ExerciseController : ControllerBase
{
    private readonly IExerciseRepository repo;
    private readonly IAppLogger logger;
    private readonly IUserService userService;

    public ExerciseController(
        IExerciseRepository repo,
        IUserService userService,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
        this.userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetExercise(
        Guid id,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Exercise id cannot be empty.");
        }

        try
        {
            var exercise = await repo.GetByIdAsync(ExerciseId.FromGuid(id), ct);

            if (exercise is null)
            {
                return NotFound();
            }

            return Ok(ExerciseMapper.ToGetDTO(exercise, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(ExerciseController),
                $"Failed to get exercise {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllExercises(
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        try
        {
            var exercises = await repo.ListAsync(ct);

            return Ok(ExerciseMapper.ToGetDTOs(exercises, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(ExerciseController),
                $"Failed to get exercises: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet("by-movement/{movementId:guid}")]
    public async Task<IActionResult> GetExercisesByMovement(
        Guid movementId,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (movementId == Guid.Empty)
        {
            return BadRequest("Movement id cannot be empty.");
        }

        try
        {
            var exercises = await repo.ListByMovementAsync(
                MovementId.FromGuid(movementId),
                ct);

            return Ok(ExerciseMapper.ToGetDTOs(exercises, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(ExerciseController),
                $"Failed to get exercises by movement {movementId}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateExercise(
        ExercisePostDTO payload,
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
                return Conflict("An exercise with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            var exercise = ExerciseMapper.ToEntity(payload, activeUser);

            var createdExercise = await repo.CreateAsync(exercise, ct);

            return CreatedAtAction(
                nameof(GetExercise),
                new { id = createdExercise.Id.Value },
                ExerciseMapper.ToGetDTO(createdExercise));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(ExerciseController),
                $"Failed to create exercise: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExercise(
        Guid id,
        ExercisePutDTO payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Exercise id cannot be empty.");
        }

        try
        {
            var exerciseId = ExerciseId.FromGuid(id);

            var exercise = await repo.GetTrackedByIdAsync(exerciseId, ct);

            if (exercise is null)
            {
                return NotFound();
            }

            var nameExists = await repo.NameExistsAsync(
                payload.Name,
                exerciseId,
                ct);

            if (nameExists)
            {
                return Conflict("Another exercise with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);
            var update = ExerciseUpdateMapper.ToUpdate(exercise, payload, activeUser);

            exercise.Update(update);

            await repo.UpdateAsync(exercise, ct);

            return Ok(ExerciseMapper.ToGetDTO(exercise));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(ExerciseController),
                $"Failed to update exercise {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }
}