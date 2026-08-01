using Microsoft.AspNetCore.Mvc;
using PRLab.API.DTO.Workout;
using PRLab.API.DTO.Workout.WorkoutBlock;
using PRLab.API.DTO.Workout.WorkoutBlockAssignment;
using PRLab.API.Mapper.UpdateMapper;
using PRLab.API.Mapper.WorkoutMappers;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Application.Interface.UserService;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.API.Controller.Entity;

[ApiController]
[Route("workouts")]
public class WorkoutController : ControllerBase
{
    private readonly IWorkoutRepository repo;
    private readonly IAppLogger logger;
    private readonly ICurrentUserService userService;
    private readonly IWorkoutBlockRepository blockRepo;

    public WorkoutController(
        IWorkoutRepository repo,
        IWorkoutBlockRepository blockRepo,
        ICurrentUserService userService,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
        this.userService = userService;
        this.blockRepo = blockRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkouts(
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        try
        {
            var workouts = await repo.ListAsync(ct);

            return Ok(WorkoutMapper.ToGetDTOs(workouts, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to get Workouts: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWorkout(
        Guid id,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Workout id cannot be empty.");
        }

        try
        {
            var workout = await repo.GetByIdAsync(WorkoutId.FromGuid(id), ct);

            if (workout is null)
            {
                return NotFound();
            }

            return Ok(WorkoutMapper.ToGetDTO(workout, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to get Workout {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyWorkouts(
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        try
        {
            // based on account type , get workout from this user , from their coach , or other public workout 
           
            var currentUser = await userService.GetCurrentUserAsync(ct);
            if (currentUser == null)
            {
                logger.Log(
                    nameof(WorkoutController),
                    $"Failed to get Workouts from current user: Current User is null");

                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    $"Failed to get Current User");
            }

            var userId = currentUser.Id;

            var workouts = await repo.ListByAuthorIdAsync(userId, ct);

            return Ok(WorkoutMapper.ToGetDTOs(workouts, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to get Workouts: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkout(
        WorkoutPostDTO payload,
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
                return Conflict("An Workout with this name already exists.");
            }

            var activeUser = await userService.GetCurrentUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            var workout = WorkoutMapper.ToEntity(payload, activeUser);

            var createdWorkout = await repo.CreateAsync(workout, ct);

            return CreatedAtAction(
                nameof(GetWorkout),
                new { id = createdWorkout.Id.Value },
                WorkoutMapper.ToGetDTO(createdWorkout));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to create Workout: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkout(
        Guid id,
        WorkoutPutDTO payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Workout id cannot be empty.");
        }

        try
        {
            var workoutId = WorkoutId.FromGuid(id);

            var workout = await repo.GetTrackedByIdAsync(workoutId, ct);

            if (workout is null)
            {
                return NotFound();
            }

            var nameExists = await repo.NameExistsAsync(
                payload.Name,
                workoutId,
                ct);

            if (nameExists)
            {
                return Conflict("Another Workout with this name already exists.");
            }

            var activeUser = await userService.GetCurrentUserAsync(ct);
            var update = WorkoutUpdateMapper.ToUpdate(workout, payload, activeUser);

            workout.Update(update);

            await repo.UpdateAsync(workout, ct);

            return Ok(WorkoutMapper.ToGetDTO(workout));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to update Workout {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWorkout(
        Guid id,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Workout id cannot be empty.");
        }

        try
        {
            var workoutId = WorkoutId.FromGuid(id);

            var workout = await repo.GetTrackedByIdAsync(workoutId, ct);

            if (workout is null)
            {
                return NotFound();
            }

            var activeUser = await userService.GetCurrentUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            workout.MarkDeleted(activeUser);

            await repo.UpdateAsync(workout, ct);

            return NoContent();
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to delete Workout {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    [HttpPost("{id:guid}/blocks")]
    public async Task<IActionResult> AddBlock(
        Guid id,
        WorkoutBlockAssignmentPostDTO payload,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Workout id cannot be empty.");
        }

        if (payload.WorkoutBlockId.Value == Guid.Empty)
        {
            return BadRequest("Workout block id cannot be empty.");
        }

        if (payload.Sequence.HasValue && payload.Sequence.Value < 1)
        {
            return BadRequest("Sequence must be greater than zero.");
        }

        try
        {
            var activeUser = await userService.GetCurrentUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            var workoutId = WorkoutId.FromGuid(id);

            var workout = await repo.GetTrackedByIdAsync(workoutId, ct);

            if (workout is null)
            {
                return NotFound();
            }

            WorkoutBlockAssignment? workoutBlockAssignment = null;
            if (payload.Block is not null)
            {
                var workoutBlock = WorkoutBlockMapper.ToEntity(payload.Block, activeUser);
                workoutBlockAssignment = WorkoutBlockMapper.ToAssignment(workoutId,  workoutBlock, payload.Sequence);
            }
            
            else {  
                
                workoutBlockAssignment = await blockRepo.GetTrackedByIdAsync(
                payload.WorkoutBlockId,
                ct);
                
            }

            if (workoutBlockAssignment is null)
            {
                return NotFound("Workout block was not found.");
            }

            var sequence = payload.Sequence ?? workout.Blocks.Count + 1;

            workout.AddBlock(
                workoutBlockAssignment,
                activeUser,
                sequence);

            await repo.UpdateAsync(workout, ct);

            return Ok(WorkoutMapper.ToGetDTO(workout, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to add block to Workout {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    [HttpDelete("{id:guid}/blocks/{assignmentId:guid}")]
    public async Task<IActionResult> RemoveBlock(
        Guid id,
        Guid assignmentId,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Workout id cannot be empty.");
        }

        if (assignmentId == Guid.Empty)
        {
            return BadRequest("Workout block assignment id cannot be empty.");
        }

        try
        {
            var activeUser = await userService.GetCurrentUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            var workoutId = WorkoutId.FromGuid(id);
            var workoutBlockAssignmentId = WorkoutBlockAssignmentId.FromGuid(assignmentId);

            var workout = await repo.GetTrackedByIdAsync(workoutId, ct);

            if (workout is null)
            {
                return NotFound();
            }

            var assignmentExists = workout.Blocks.Any(
                blockAssignment => blockAssignment.Id == workoutBlockAssignmentId);

            if (!assignmentExists)
            {
                return NotFound("Workout block assignment was not found.");
            }

            workout.RemoveBlock(
                workoutBlockAssignmentId,
                activeUser);

            await repo.UpdateAsync(workout, ct);

            return Ok(WorkoutMapper.ToGetDTO(workout, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to remove block assignment {assignmentId} from Workout {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    [HttpPut("{id:guid}/blocks/order")]
    public async Task<IActionResult> ReorderBlocks(
        Guid id,
        WorkoutBlockOrderPutDTO payload,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Workout id cannot be empty.");
        }

        if (payload.Blocks.Count == 0)
        {
            return BadRequest("At least one block assignment is required.");
        }

        if (payload.Blocks.Any(block => block.AssignmentId.Value == Guid.Empty))
        {
            return BadRequest("Workout block assignment id cannot be empty.");
        }

        if (payload.Blocks.Any(block => block.Sequence < 1))
        {
            return BadRequest("Sequence must be greater than zero.");
        }

        var duplicateAssignmentExists = payload.Blocks
            .GroupBy(block => block.AssignmentId)
            .Any(group => group.Count() > 1);

        if (duplicateAssignmentExists)
        {
            return BadRequest("Duplicate workout block assignment ids are not allowed.");
        }

        var duplicateSequenceExists = payload.Blocks
            .GroupBy(block => block.Sequence)
            .Any(group => group.Count() > 1);

        if (duplicateSequenceExists)
        {
            return BadRequest("Duplicate block sequences are not allowed.");
        }

        try
        {
            var activeUser = await userService.GetCurrentUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            var workoutId = WorkoutId.FromGuid(id);

            var workout = await repo.GetTrackedByIdAsync(workoutId, ct);

            if (workout is null)
            {
                return NotFound();
            }

            if (payload.Blocks.Count != workout.Blocks.Count)
            {
                return BadRequest("Reorder payload must include every workout block assignment.");
            }

            var existingAssignmentIds = workout.Blocks
                .Select(blockAssignment => blockAssignment.Id)
                .ToHashSet();

            var missingAssignmentExists = payload.Blocks
                .Any(block => !existingAssignmentIds.Contains(block.AssignmentId));

            if (missingAssignmentExists)
            {
                return BadRequest("One or more block assignments do not belong to this workout.");
            }

            var expectedSequences = Enumerable
                .Range(1, workout.Blocks.Count)
                .ToHashSet();

            var requestedSequences = payload.Blocks
                .Select(block => block.Sequence)
                .ToHashSet();

            if (!expectedSequences.SetEquals(requestedSequences))
            {
                return BadRequest("Block sequences must be contiguous and start at 1.");
            }

            foreach (var block in payload.Blocks.OrderBy(block => block.Sequence))
            {
                workout.MoveBlock(
                    block.AssignmentId,
                    block.Sequence,
                    activeUser);
            }

            await repo.UpdateAsync(workout, ct);

            return Ok(WorkoutMapper.ToGetDTO(workout, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(WorkoutController),
                $"Failed to reorder blocks for Workout {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

}