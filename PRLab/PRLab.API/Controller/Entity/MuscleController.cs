using Microsoft.AspNetCore.Mvc;
using PRLab.API.DTO.Muscle;
using PRLab.API.DTO.Muscle.Relation;
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
[Route("muscles")]
public sealed class MuscleController : ControllerBase
{
    private readonly IMuscleRepository repo;
    private readonly IAppLogger logger;
    private readonly IUserService userService;

    public MuscleController(
        IMuscleRepository repo,
        IUserService userService,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
        this.userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMuscle(
        Guid id,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Muscle id cannot be empty.");
        }

        try
        {
            var muscle = await repo.GetByIdAsync(MuscleId.FromGuid(id), ct);

            if (muscle is null)
            {
                return NotFound();
            }

            return Ok(MuscleMapper.ToGetDTO(muscle, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MuscleController),
                $"Failed to get muscle {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMuscles(
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        try
        {
            var muscles = await repo.ListAsync(ct);

            return Ok(MuscleMapper.ToGetDTOs(muscles, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MuscleController),
                $"Failed to get muscles: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred.{exception.GetBaseException().Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateMuscle(
        MusclePostDTO payload,
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
                return Conflict("A muscle with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);
            var muscle = MuscleMapper.ToEntity(payload, activeUser);

            var createdMuscle = await repo.CreateAsync(muscle, ct);

            return CreatedAtAction(
                nameof(GetMuscle),
                new { id = createdMuscle.Id.Value },
                MuscleMapper.ToGetDTO(createdMuscle));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MuscleController),
                $"Failed to create muscle: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMuscle(
        Guid id,
        MusclePutDTO payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Muscle id cannot be empty.");
        }

        try
        {
            var muscleId = MuscleId.FromGuid(id);

            var muscle = await repo.GetByIdAsync(muscleId, ct);

            if (muscle is null)
            {
                return NotFound();
            }

            var nameExists = await repo.NameExistsAsync(
                payload.Name,
                muscleId,
                ct);

            if (nameExists)
            {
                return Conflict("Another muscle with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);
            var update = MuscleUpdateMapper.ToUpdate(payload, activeUser);

            muscle.Update(update);

            await repo.UpdateAsync(muscle, ct);

            return Ok(MuscleMapper.ToGetDTO(muscle));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MuscleController),
                $"Failed to update muscle {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPost("{id:guid}/antagonists/{antagonistId:guid}")]
    public async Task<IActionResult> AddAntagonist(
        Guid id,
        Guid antagonistId,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Muscle id cannot be empty.");
        }

        if (antagonistId == Guid.Empty)
        {
            return BadRequest("Antagonist muscle id cannot be empty.");
        }

        if (id == antagonistId)
        {
            return BadRequest("A muscle cannot be its own antagonist.");
        }

        try
        {
            var muscleId = MuscleId.FromGuid(id);
            var antagonistMuscleId = MuscleId.FromGuid(antagonistId);

            var muscle = await repo.GetByIdAsync(muscleId, ct);

            if (muscle is null)
            {
                return NotFound("Muscle was not found.");
            }

            var antagonistExists = await repo.ExistsAsync(antagonistMuscleId, ct);

            if (!antagonistExists)
            {
                return NotFound("Antagonist muscle was not found.");
            }

            muscle.AddAntagonist(antagonistMuscleId);

            await repo.UpdateAsync(muscle, ct);

            return Ok(MuscleMapper.ToGetDTO(muscle));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MuscleController),
                $"Failed to add antagonist {antagonistId} to muscle {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpDelete("{id:guid}/antagonists/{antagonistId:guid}")]
    public async Task<IActionResult> RemoveAntagonist(
        Guid id,
        Guid antagonistId,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Muscle id cannot be empty.");
        }

        if (antagonistId == Guid.Empty)
        {
            return BadRequest("Antagonist muscle id cannot be empty.");
        }

        try
        {
            var muscle = await repo.GetByIdAsync(MuscleId.FromGuid(id), ct);

            if (muscle is null)
            {
                return NotFound();
            }

            muscle.RemoveAntagonist(MuscleId.FromGuid(antagonistId));

            await repo.UpdateAsync(muscle, ct);

            return Ok(MuscleMapper.ToGetDTO(muscle));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MuscleController),
                $"Failed to remove antagonist {antagonistId} from muscle {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPut("{id:guid}/antagonists")]
    public async Task<IActionResult> UpdateAntagonists(
        Guid id,
        MuscleAntagonistsPutDTO payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Muscle id cannot be empty.");
        }

        if (payload.AntagonistIds.Any(antagonistId => antagonistId.Value == Guid.Empty))
        {
            return BadRequest("Antagonist ids cannot contain empty values.");
        }

        if (payload.AntagonistIds.Any(antagonistId => antagonistId.Value == id))
        {
            return BadRequest("A muscle cannot be its own antagonist.");
        }

        try
        {
            var muscleId = MuscleId.FromGuid(id);

            var muscleExists = await repo.ExistsAsync(muscleId, ct);

            if (!muscleExists)
            {
                return NotFound("Muscle was not found.");
            }

            var distinctAntagonistIds = payload.AntagonistIds
                .Distinct()
                .ToList();

            var allAntagonistsExist = await repo.AllExistAsync(distinctAntagonistIds, ct);

            if (!allAntagonistsExist)
            {
                return NotFound("One or more antagonist muscles were not found.");
            }

            var updatedMuscle = await repo.UpdateAntagonistsAsync(
                muscleId,
                distinctAntagonistIds,
                ct);

            return Ok(MuscleMapper.ToGetDTO(updatedMuscle));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MuscleController),
                $"Failed to update antagonists for muscle {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }
}