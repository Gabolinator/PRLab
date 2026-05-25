namespace PRLab.API.Dtos.UpdateDto.Request;

public sealed record DescriptorUpdateRequest( 
    DescriptorUpdateDTO Descriptor,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);

    
