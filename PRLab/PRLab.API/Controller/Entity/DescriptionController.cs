using Microsoft.AspNetCore.Mvc;
using PRLab.API.Dtos.PostDto;
using PRLab.API.Dtos.PutDto;
using PRLab.API.Mapper;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Controller.Entity;

[ApiController]
[Route("descriptions")]
public sealed class DescriptionController : ControllerBase
{
    private readonly IDescriptionRepository repo;
    private readonly IAppLogger logger;

    public DescriptionController(
        IDescriptionRepository repo,
        IAppLogger logger)
    {
        this.repo = repo;
        this.logger = logger;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDescription(
        Guid id,
        [FromQuery] string? languageCode = null,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Description id cannot be empty.");
        }

        try
        {
            var description = await repo.GetByIdAsync(
                DescriptionId.FromGuid(id),
                ct);

            if (description is null)
            {
                return NotFound();
            }

            return Ok(DescriptionMapper.ToGetDTO(description, languageCode));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(DescriptionController),
                $"Failed to get description {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }
    
    [HttpGet()]
    public async Task<IActionResult> GetAllDescriptions(
        [FromQuery] LocalizationHelper.Language? languageCode = null,
        CancellationToken ct = default)
    {
        try
        {
            var descriptions = await repo.ListAsync(ct);
            return Ok(DescriptionMapper.ToGetDTOs(descriptions.ToList(), LocalizationHelper.ValidateLanguageOrDefault(languageCode)));
        }
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred. {exception.GetBaseException().Message}");
        }
    }


    [HttpPost]
    public async Task<IActionResult> PostDescription(
        [FromBody] DescriptionPostDTO? payload,
        CancellationToken ct = default)
    {
        if (payload is null)
        {
            return BadRequest("Payload cannot be null.");
        }

        try
        {
            var description = DescriptionMapper.ToEntity(payload);

            var createdDescription = await repo.CreateAsync(description, ct);

            var response = DescriptionMapper.ToGetDTO(createdDescription, payload.Language);

            return CreatedAtAction(
                nameof(GetDescription),
                new { id = response.Id },
                response);
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(DescriptionController),
                $"Failed to create description: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"An unexpected error occurred {exception.GetBaseException().Message}.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutDescription(
        Guid id,
        [FromBody] DescriptionPutDTO? payload,
        CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Description id cannot be empty.");
        }

        if (payload is null)
        {
            return BadRequest("Payload cannot be null.");
        }

        try
        {
            var description = await repo.GetByIdAsync(
                DescriptionId.FromGuid(id),
                ct);

            if (description is null)
            {
                return NotFound();
            }

            description.ChangeContent(payload.Content);

            var updatedDescription = await repo.UpdateAsync(description, ct);

            return Ok(DescriptionMapper.ToGetDTO(updatedDescription));
        }
        catch (Exception exception)
        {
            logger.Log(
                nameof(DescriptionController),
                $"Failed to update description {id}: {exception.Message}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }
}