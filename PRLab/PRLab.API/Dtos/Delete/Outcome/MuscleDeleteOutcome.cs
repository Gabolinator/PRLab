using PRLab.API.Dtos.ID;

namespace PRLab.API.Dtos.Delete.Outcome;

public sealed record MuscleDeleteOutcome(EntityId Id, DeleteOutcome Outcome);
