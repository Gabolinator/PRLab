using PRLab.Model.Entity;
using PRLab.Value.Identifier;

namespace PRLab.Model.Join;

public sealed class MovementEquipment
{
    public MovementId MovementId { get; private init; }

    public Movement Movement { get; private init; } = null!;

    public EquipmentId EquipmentId { get; private init; }

    public Equipment Equipment { get; private init; } = null!;

    private MovementEquipment()
    {
        // EF Core
    }

    private MovementEquipment(
        MovementId movementId,
        EquipmentId equipmentId)
    {
        MovementId = movementId;
        EquipmentId = equipmentId;
    }

    public static MovementEquipment New(
        MovementId movementId,
        EquipmentId equipmentId)
    {
        return new MovementEquipment(
            movementId,
            equipmentId
        );
    }
}