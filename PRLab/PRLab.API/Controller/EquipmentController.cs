using Microsoft.AspNetCore.Mvc;
using PRLab.API.Dtos.PostDto;
using PRLab.API.Dtos.PutDto;
using PRLab.API.Mapper;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Update;

namespace PRLab.API.Controller;

[ApiController]
[Route("equipments")]
public sealed class EquipmentController : ControllerBase
{
    private readonly IEquipmentRepository repo;
    private readonly IAppLogger logger;

    public EquipmentController(
        IEquipmentRepository repo,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
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
            var equipment = EquipmentMapper.ToEntity(payload);

            var createdEquipment = await repo.CreateAsync(equipment, ct);

            var response = EquipmentMapper.ToGetDTO(createdEquipment);

            return CreatedAtAction(
                nameof(GetEquipment),
                new { id = response.Id },
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
            var equipment = await repo.GetByIdAsync(
                EquipmentId.FromGuid(id),
                ct);

            if (equipment is null)
            {
                return NotFound();
            }

            equipment.Update(EquipmentUpdateMapper.ToUpdate(payload));

            var updatedEquipment = await repo.UpdateAsync(equipment, ct);

            return Ok(EquipmentMapper.ToGetDTO(updatedEquipment));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(EquipmentController),
                $"Failed to update Equipment {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }
}



// using GainsLab.Application.Interfaces;
// using GainsLab.Application.Interfaces.DataManagement.Repository;
// using GainsLab.Application.Results.APIResults;
// using GainsLab.Contracts.Dtos.GetDto;
// using GainsLab.Contracts.Dtos.PostDto;
// using GainsLab.Contracts.Dtos.PutDto;
// using GainsLab.Contracts.Dtos.SyncDto;
// using GainsLab.Contracts.Dtos.UpdateDto;
// using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
// using GainsLab.Domain.Entities.Identifier;
// using GainsLab.Infrastructure.SyncService;
// using Microsoft.AspNetCore.Mvc;
//
//
// namespace GainsLab.Api.Controller;
//
// [ApiController]
// [Route("equipments")]
// public class EquipmentController : ControllerBase
// {
//     private readonly IEquipmentRepository _repo;
//     private readonly ISyncService<EquipmentSyncDTO> _svc;
//    
//
//     public EquipmentController(IEquipmentRepository repo, ISyncService<EquipmentSyncDTO> svc)
//     {
//         _repo = repo;
//         _svc = svc;
//     }
//
//
//      [HttpGet("sync")]
//     public async Task<IActionResult> GetEquipments(
//         [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
//     {
//         
//         var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
//         take = Math.Clamp(take, 1, 500);
//
//         var page = await _svc.PullAsync(cursor, take, ct);
//         return Ok(page);
//     }
//     
//     [HttpGet("{id:guid}")]
//     public async Task<IActionResult> GetEquipment(
//         Guid id, CancellationToken ct = default)
//     {
//         
//         
//         if( id == Guid.Empty)  return BadRequest();
//         
//         var result = await _repo.PullByIdAsync(EquipmentId.FromGuid(id), ct);
//
//         return  APIResultValidation.ValidateResult<EquipmentGetDTO>(this,result);
//     }
//     
//        
//     [HttpPost()]
//     public async Task<IActionResult> PostEquipment(
//         [FromBody] EquipmentPostDTO? payload, CancellationToken ct = default)
//     {
//         
//         if(payload == null)  return BadRequest();
//         
//         var result = await _repo.PostAsync(payload,ct);
//    
//         return APIResultValidation.ValidateResult<EquipmentGetDTO>(this, result,
//             result.Value != null ? new ActionResultInfo(GetActionName(), result.Value.Id) : null);
//     }
//
//     [HttpPut("{id:guid}")]
//     public async Task<IActionResult> PutEquipment(
//        Guid id, [FromBody] EquipmentPutDTO? payload  , CancellationToken ct = default)
//     {
//         if(payload == null)  return BadRequest();
//         
//         var result = await _repo.PutAsync(EquipmentId.FromGuid(id),payload,ct);
//         
//         return  APIResultValidation.ValidateResult<EquipmentPutDTO>(this,result, new ActionResultInfo(GetActionName(),id));
//         
//     }
//     
//     
//     [HttpPatch("{id:guid}")] 
//     public async Task<IActionResult> PatchEquipment(
//         Guid id, [FromBody] EquipmentUpdateDTO? payload, CancellationToken ct = default)
//     {
//         if(payload == null|| id == Guid.Empty)  return BadRequest();
//         
//         var result = await _repo.PatchAsync(EquipmentId.FromGuid(id),payload,ct);
//         return  APIResultValidation.ValidateResult<EquipmentUpdateOutcome>(this,result);
//     
//     }
//     
//    
//
//     [HttpDelete("{id:guid}")]
//     public async Task<IActionResult> DeleteEquipment(Guid id, CancellationToken ct = default)
//     {
//         if (id == Guid.Empty) return BadRequest();
//
//         var result = await _repo.DeleteAsync(EquipmentId.FromGuid(id), ct);
//         return APIResultValidation.ValidateResult<EquipmentGetDTO>(this, result);
//     }
//
//     private string GetActionName() => nameof(GetEquipment);
//     
// }
