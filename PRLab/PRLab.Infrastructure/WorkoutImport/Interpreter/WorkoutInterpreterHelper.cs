namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

public static class WorkoutInterpreterHelper
{ 
    public static IReadOnlyList<string> GetExercisesNames(IReadOnlyDictionary<string, string> exerciseAliases, IReadOnlyList<string> allParts)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<string> GetAllParts(string exerciseCellValue, IReadOnlyList<string> separators)
    {
        var allParts = new List<string>();
        foreach (var separator in separators)
        {
            var parts = exerciseCellValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            allParts.AddRange(parts.Distinct());
        }
        
        return allParts
            .Distinct()
            .ToList();
    }

    public static bool IsSameDate(string value, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }
        
        return DateTime.TryParse(value, out var cellDate) && cellDate.Date == date;
    }

    public static IReadOnlyList<DateTime> GetDistinctDatesFromStrings(IEnumerable<string> values)
    {
        return values
            .Select(value =>
                DateTime.TryParse(
                    value,
                    out var date)
                    ? date.Date
                    : (DateTime?)null)
            .Where(date => date.HasValue)
            .Select(date => date!.Value)
            .Distinct()
            .OrderBy(date => date)
            .ToList();
    }
}