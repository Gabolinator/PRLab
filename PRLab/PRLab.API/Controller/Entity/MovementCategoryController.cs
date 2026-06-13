using Microsoft.AspNetCore.Mvc;
using PRLab.API.DTO.MovementCategory;
using PRLab.API.Mapper;
using PRLab.API.Mapper.UpdateMapper;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Controller.Entity;

[ApiController]
[Route("movement-categories")]
public sealed class MovementCategoryController : ControllerBase
{
    private readonly IMovementCategoryRepository repo;
    private readonly IAppLogger logger;
    private readonly IUserService userService;

    public MovementCategoryController(
        IMovementCategoryRepository repo,
        IUserService userService,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
        this.userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMovementCategory(
        Guid id,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Movement category id cannot be empty.");
        }

        try
        {
            var movementCategory = await repo.GetByIdAsync(
                MovementCategoryId.FromGuid(id),
                ct);

            if (movementCategory is null)
            {
                return NotFound();
            }

            return Ok(MovementCategoryMapper.ToGetDTO(movementCategory, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementCategoryController),
                $"Failed to get movement category {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMovementCategories(
        [FromQuery] LocalizationHelper.Language? language = null,
        [FromQuery] DomainEnum.BaseMovementCategory? baseCategory = null,
        CancellationToken ct = default)
    {
        try
        {
            var movementCategories = baseCategory.HasValue
                ? await repo.ListByBaseCategoryAsync(baseCategory.Value, ct)
                : await repo.ListAsync(ct);

            return Ok(MovementCategoryMapper.ToGetDTOs(movementCategories, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementCategoryController),
                $"Failed to get movement categories: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpGet("by-name/{name}")]
    public async Task<IActionResult> GetMovementCategoryByName(
        string name,
        [FromQuery] LocalizationHelper.Language? language = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Movement category name cannot be empty.");
        }

        try
        {
            var movementCategory = await repo.GetByNameAsync(name, ct);

            if (movementCategory is null)
            {
                return NotFound();
            }

            return Ok(MovementCategoryMapper.ToGetDTO(movementCategory, language));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementCategoryController),
                $"Failed to get movement category by name {name}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovementCategory(
        MovementCategoryPostDTO payload,
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
                return Conflict("A movement category with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);

            if (activeUser is null)
            {
                return Unauthorized();
            }

            var movementCategory = MovementCategoryMapper.ToEntity(payload, activeUser);

            var createdMovementCategory = await repo.CreateAsync(movementCategory, ct);

            return CreatedAtAction(
                nameof(GetMovementCategory),
                new { id = createdMovementCategory.Id.Value },
                MovementCategoryMapper.ToGetDTO(createdMovementCategory));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementCategoryController),
                $"Failed to create movement category: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMovementCategory(
        Guid id,
        MovementCategoryPutDTO payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Movement category id cannot be empty.");
        }

        try
        {
            var movementCategoryId = MovementCategoryId.FromGuid(id);

            var movementCategory = await repo.GetByIdAsync(movementCategoryId, ct);

            if (movementCategory is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(payload.Name))
            {
                var nameExists = await repo.NameExistsAsync(
                    payload.Name,
                    movementCategoryId,
                    ct);

                if (nameExists)
                {
                    return Conflict("Another movement category with this name already exists.");
                }
            }

            var activeUser = await userService.GetActiveUserAsync(ct);
            var update = MovementCategoryUpdateMapper.ToUpdate(payload, activeUser);

            movementCategory.Update(update);

            await repo.UpdateAsync(movementCategory, ct);

            return Ok(MovementCategoryMapper.ToGetDTO(movementCategory));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(MovementCategoryController),
                $"Failed to update movement category {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }
}