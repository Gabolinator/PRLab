using System.Drawing;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.Utilities;

/// <summary>
/// Console-based logger with colored output for PRLab components.
/// </summary>
public class PRLabLogger : IAppLogger
{
    private static readonly Color DefaultMessageColor = Color.White;
    private static readonly Color DefaultHeaderColor = Color.Green;
    private static readonly Color DefaultContextColor = Color.Cyan;
    private static readonly Color DefaultWarningColor = Color.Yellow;
    private static readonly Color DefaultErrorColor = Color.Red;

    private bool enabled = true;
    private bool decorate = true;

    private readonly Color currentHeaderColor = DefaultHeaderColor;

    /// <summary>
    /// Gets the display name of the logger.
    /// </summary>
    public readonly string LoggerName = "Logger";

    /// <inheritdoc />
    public bool Enabled => enabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="PRLabLogger"/> class using the default name.
    /// </summary>
    public PRLabLogger()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PRLabLogger"/> class with a custom name.
    /// </summary>
    /// <param name="name">The label displayed in logging output.</param>
    public PRLabLogger(string name)
    {
        LoggerName = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PRLabLogger"/> class with custom name and header color.
    /// </summary>
    /// <param name="name">The label displayed in logging output.</param>
    /// <param name="currentHeaderColor">The color applied to the logger header.</param>
    public PRLabLogger(string name, Color currentHeaderColor)
    {
        LoggerName = name;
        this.currentHeaderColor = currentHeaderColor;
        decorate = currentHeaderColor != Color.Empty && currentHeaderColor != Color.Transparent;
    }

    public void ToggleDecoration(bool state)
    {
        decorate = state;
    }

    /// <inheritdoc />
    public void ToggleLogging(bool state)
    {
        WriteLog(
            null,
            $"Toggled logging {state}",
            null,
            DefaultMessageColor,
            currentHeaderColor,
            DefaultContextColor);

        enabled = state;
    }

    /// <inheritdoc />
    public void Log(string message)
    {
        Log(
            null,
            message,
            DefaultMessageColor,
            currentHeaderColor);
    }

    /// <inheritdoc />
    public void Log(
        string context,
        string message)
    {
        Log(
            context,
            message,
            DefaultMessageColor,
            currentHeaderColor,
            DefaultContextColor);
    }

    public void Log(
        string message,
        Color? messageColor,
        Color? headerColor = null)
    {
        WriteLog(
            null,
            message,
            null,
            messageColor,
            headerColor,
            null);
    }

    public void Log(
        string? context,
        string message,
        Color? messageColor,
        Color? headerColor = null,
        Color? contextColor = null)
    {
        WriteLog(
            context,
            message,
            null,
            messageColor,
            headerColor,
            contextColor);
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
        WriteLog(
            null,
            message,
            IAppLogger.AppLogLevel.Warning,
            DefaultWarningColor,
            currentHeaderColor,
            DefaultContextColor,
            DefaultWarningColor);
    }

    /// <inheritdoc />
    public void LogWarning(string context, string message)
    {
        WriteLog(
            context,
            message,
            IAppLogger.AppLogLevel.Warning,
            DefaultWarningColor,
            currentHeaderColor,
            DefaultContextColor,
            DefaultWarningColor);
    }

    /// <inheritdoc />
    public void LogError(string message)
    {
        WriteLog(
            null,
            message,
            IAppLogger.AppLogLevel.Error,
            DefaultErrorColor,
            currentHeaderColor,
            DefaultContextColor,
            DefaultErrorColor);
    }

    /// <inheritdoc />
    public void LogError(string context, string message)
    {
        WriteLog(
            context,
            message,
            IAppLogger.AppLogLevel.Error,
            DefaultErrorColor,
            currentHeaderColor,
            DefaultContextColor,
            DefaultErrorColor);
    }

    private void WriteLog(
        string? context,
        string message,
        IAppLogger.AppLogLevel? level,
        Color? messageColor = null,
        Color? headerColor = null,
        Color? contextColor = null,
        Color? levelColor = null)
    {
        if (!Enabled)
        {
            return;
        }

        var resolvedMessageColor = GetColorString(messageColor ?? DefaultMessageColor);
        var resolvedHeaderColor = GetColorString(headerColor ?? this.currentHeaderColor);
        var resolvedContextColor = GetColorString(contextColor ?? DefaultContextColor);
        var resolvedLevelColor = GetColorString(levelColor ?? GetDefaultLevelColor(level));
        
        var resetColor = GetResetColorString();

        bool hasContext = !string.IsNullOrWhiteSpace(context);
        bool hasLevel = level != null && level != IAppLogger.AppLogLevel.Info;
        
        var levelText = !hasLevel
            ? string.Empty
            : $"{resolvedLevelColor} {level} {(hasContext ? "" : " -")}";

        var contextText = !hasContext
            ? string.Empty
            : $" - {resolvedContextColor}[{context}]";

        var headerText = $"{resolvedHeaderColor}[{LoggerName}]";
        
        Console.WriteLine(
            $"{headerText}{levelText} {contextText} {resolvedMessageColor}{message}{resetColor}");
    }

    private Color GetDefaultLevelColor(IAppLogger.AppLogLevel? level) =>
        level switch
        {
            IAppLogger.AppLogLevel.Info => DefaultMessageColor,
            IAppLogger.AppLogLevel.Warning => DefaultWarningColor,
            IAppLogger.AppLogLevel.Error => DefaultErrorColor,
            null => DefaultMessageColor,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
   

    private string GetColorString(Color color)
    {
        if (!decorate)
        {
            return string.Empty;
        }

        if (color == Color.Empty || color == Color.Transparent)
        {
            return string.Empty;
        }

        if (color == Color.White)
        {
            return "\u001b[37m";
        }

        if (color == Color.Cyan)
        {
            return "\u001b[36m";
        }

        if (color == Color.Green)
        {
            return "\u001b[32m";
        }

        if (color == Color.Yellow)
        {
            return "\u001b[33m";
        }

        if (color == Color.Red)
        {
            return "\u001b[31m";
        }

        return $"\u001b[38;2;{color.R};{color.G};{color.B}m";
    }

    private string GetResetColorString()
    {
        if (!decorate)
        {
            return string.Empty;
        }

        return "\u001b[0m";
    }
}