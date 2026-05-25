using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.PostDto.Outcome;

public record EquipmentCreateOutcome(CreateOutcome Outcome,EquipmentGetDTO? CreatedEquipment,  IMessagesContainer? Message =null);


