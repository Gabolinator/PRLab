using PRLab.API.Dtos.SummaryDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.PostDto.Outcome;

public sealed record DescriptionCreateOutcome(CreateOutcome Outcome,DescriptionSummaryDTO? CreatedDescriptor,  IMessagesContainer? Message =null);

