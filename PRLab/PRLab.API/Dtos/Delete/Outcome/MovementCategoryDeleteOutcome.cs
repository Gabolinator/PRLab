using PRLab.API.Dtos.ID;

namespace PRLab.API.Dtos.Delete.Outcome;

public sealed record MovementCategoryDeleteOutcome(EntityId Id, DeleteOutcome Outcome);
