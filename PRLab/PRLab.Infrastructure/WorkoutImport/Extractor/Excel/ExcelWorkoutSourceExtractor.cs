using System.Globalization;
using System.Text;
using ExcelDataReader;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

public sealed class ExcelWorkoutSourceExtractor(
    IAppLogger logger)
    : IWorkoutSourceExtractor
{
    private static readonly HashSet<string> SupportedExtensions =
    [
        ".xls",
        ".xlsx",
        ".xlsb"
    ];

    public bool CanHandle(WorkoutImportSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.Type != WorkoutImportSourceType.File ||
            string.IsNullOrWhiteSpace(source.FileName) ||
            source.Content is null)
        {
            return false;
        }

        var extension = Path.GetExtension(source.FileName);

        return SupportedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    public Task<IExtractedWorkoutSource> ExtractAsync(
        WorkoutImportSource source,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!CanHandle(source))
        {
            throw new ArgumentException(
                "Unsupported workout source.",
                nameof(source));
        }

        logger.Log(
            nameof(ExcelWorkoutSourceExtractor),
            $"Extracting workout source: {source.FileName}");

        using var stream = new MemoryStream(source.Content!);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var sections = new List<ExtractedWorkoutSheet>();
        var normalizedText = new StringBuilder();

        var sheetSequence = 1;

        do
        {
            ct.ThrowIfCancellationRequested();

            var section = ExtractSheet(
                reader,
                sheetSequence,
                ct);

            if (section is not null)
            {
                sections.Add(section);

                if (normalizedText.Length > 0)
                {
                    normalizedText.AppendLine();
                    normalizedText.AppendLine();
                }

                normalizedText.AppendLine(
                    $"## {section.Name}");

                normalizedText.Append(section.RawText);
            }

            sheetSequence++;
        }
        while (reader.NextResult());

        IExtractedWorkoutSource extractedWorkoutSource = new ExtractedExcelWorkoutSource
        {
            SourceType = WorkoutImportSourceType.File,
            SourceName = source.SourceReference ?? source.FileName,
            NormalizedText = normalizedText.ToString().Trim(),
            Sheets = sections,
            Metadata = new Dictionary<string, string>
            {
                ["FileName"] = source.FileName!,
                ["SheetCount"] = sections.Count.ToString()
            }
        };
        
        return Task.FromResult(extractedWorkoutSource);
    }

    private static ExtractedWorkoutSheet? ExtractSheet(
        IExcelDataReader reader,
        int sequence,
        CancellationToken ct)
    {
        var rows = new List<ExtractedWorkoutRow>();
        var rawText = new StringBuilder();

        var rowNumber = 0;

        while (reader.Read())
        {
            ct.ThrowIfCancellationRequested();

            rowNumber++;

            var cells = ReadRow(
                reader,
                rowNumber);

            if (cells.Count == 0)
            {
                continue;
            }

            var rowText = string.Join(
                " | ",
                cells.Select(cell => cell.Value));

            rawText.AppendLine(rowText);

            rows.Add(
                new ExtractedWorkoutRow
                {
                    RowNumber = rowNumber,
                    Cells = cells,
                    RawText = rowText
                });
        }

        if (rows.Count == 0)
        {
            return null;
        }

        return new ExtractedWorkoutSheet
        {
            Name = reader.Name,
            Sequence = sequence,
            RawText = rawText.ToString().Trim(),
            Rows = rows,
            SourceReference = reader.Name
        };
    }

    private static IReadOnlyList<ExtractedWorkoutCell> ReadRow(
        IExcelDataReader reader,
        int rowNumber)
    {
        var cells = new List<ExtractedWorkoutCell>();

        for (var columnIndex = 0;
             columnIndex < reader.FieldCount;
             columnIndex++)
        {
            var rawValue = reader.GetValue(columnIndex);

            if (rawValue is null)
            {
                continue;
            }

            var value = FormatCellValue(rawValue);

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var columnName = GetExcelColumnName(columnIndex);

            cells.Add(
                new ExtractedWorkoutCell
                {
                    ColumnIndex = columnIndex,
                    ColumnName = columnName,
                    Value = value,
                    SourceReference =
                        $"{reader.Name}!{columnName}{rowNumber}"
                });
        }

        return cells;
    }

    private static string FormatCellValue(object value)
    {
        return value switch
        {
            DateTime dateTime =>
                dateTime.ToString(
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture),

            double number =>
                number.ToString(
                    CultureInfo.InvariantCulture),

            float number =>
                number.ToString(
                    CultureInfo.InvariantCulture),

            decimal number =>
                number.ToString(
                    CultureInfo.InvariantCulture),

            bool boolean =>
                boolean ? "true" : "false",

            _ =>
                value.ToString()?.Trim()
                ?? string.Empty
        };
    }

    private static string GetExcelColumnName(
        int columnIndex)
    {
        var columnNumber = columnIndex + 1;
        var columnName = string.Empty;

        while (columnNumber > 0)
        {
            columnNumber--;

            columnName =
                (char)('A' + columnNumber % 26) +
                columnName;

            columnNumber /= 26;
        }

        return columnName;
    }
}