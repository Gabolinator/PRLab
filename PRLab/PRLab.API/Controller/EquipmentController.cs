using Microsoft.AspNetCore.Mvc;
using PRLab.API.Dtos.PostDto;
using PRLab.API.Dtos.PutDto;
using PRLab.API.Mapper;
using PRLab.API.Mapper.UpdateMapper;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Identifier;


namespace PRLab.API.Controller;

[ApiController]
[Route("equipments")]
public sealed class EquipmentController : ControllerBase
{
    private readonly IEquipmentRepository repo;
    private readonly IAppLogger logger;
    private readonly IUserService userService;
    
    public EquipmentController(
        IEquipmentRepository repo,
        IUserService userService,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
        this.userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEquipment(
        Guid id,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Equipment id cannot be empty.");
        }

        try
        {
            var equipment = await repo.GetByIdAsync(EquipmentId.FromGuid(id), ct);

            if (equipment is null)
            {
                return NotFound();
            }

            return Ok(EquipmentMapper.ToGetDTO(equipment));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(EquipmentController),
                $"Failed to get Equipment {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }
    
    [HttpGet()]
    public async Task<IActionResult> GetAllEquipments(
        CancellationToken ct = default)
    {
        try
        {
            var equipments = await repo.ListAsync(ct);

            return Ok(EquipmentMapper.ToGetDTOs(equipments));
        }
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }


    [HttpPost]
    public async Task<IActionResult> PostEquipment(
        [FromBody] EquipmentPostDTO? payload,
        CancellationToken ct = default)
    {
        if (payload is null)
        {
            return BadRequest("Payload cannot be null.");
        }

        try
        {
            var nameExists = await repo.NameExistsAsync(
                payload.Name,
                null,
                ct);

            if (nameExists)
            {
                return Conflict("An equipment with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);

            var equipment = EquipmentMapper.ToEntity(
                payload,
                activeUser);

            var createdEquipment = await repo.CreateAsync(
                equipment,
                ct);

            var response = EquipmentMapper.ToGetDTO(createdEquipment);

            return CreatedAtAction(
                nameof(GetEquipment),
                new { id = response.Id.Value },
                response);
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(EquipmentController),
                $"Failed to create Equipment: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred {exception.GetBaseException().Message}.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutEquipment(
        Guid id,
        [FromBody] EquipmentPutDTO? payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Equipment id cannot be empty.");
        }

        if (payload is null)
        {
            return BadRequest("Payload cannot be null.");
        }

        try
        {
            var equipmentId = EquipmentId.FromGuid(id);

            var equipment = await repo.GetByIdAsync(
                equipmentId,
                ct);

            if (equipment is null)
            {
                return NotFound();
            }

            var nameExists = await repo.NameExistsAsync(
                payload.Name,
                equipmentId,
                ct);

            if (nameExists)
            {
                return Conflict("Another equipment with this name already exists.");
            }

            var activeUser = await userService.GetActiveUserAsync(ct);

            equipment.Update(
                EquipmentUpdateMapper.ToUpdate(
                    payload,
                    activeUser));

            var updatedEquipment = await repo.UpdateAsync(
                equipment,
                ct);

            return Ok(EquipmentMapper.ToGetDTO(updatedEquipment));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(EquipmentController),
                $"Failed to update Equipment {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }
}