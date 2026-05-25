namespace PRLab.Application.Results;

public interface IResult
{
    bool Success { get; }
    MessagesContainer Messages { get; }
}