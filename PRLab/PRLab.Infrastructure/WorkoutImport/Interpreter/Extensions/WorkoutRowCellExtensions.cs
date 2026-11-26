namespace PRLab.Infrastructure.WorkoutImport.Interpreter.Extensions;

internal static class WorkoutRowCellExtensions
{
    public static bool TryConsumeOrder(this WorkoutRowCell? cell, out int order)
    {
        order = default;
        if (cell is null || cell.Consumption.Count > 0 || !int.TryParse(cell.Text, out order))
        {
            return false;
        }

        var orderSpan = new TextSpan(0, cell.Text.Length);
        cell.Consume(orderSpan, ConsumptionKind.Order);
        return true;
    }
}
