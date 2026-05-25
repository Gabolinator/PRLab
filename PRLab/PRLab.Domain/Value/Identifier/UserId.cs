namespace PRLab.Domain.Value.Identifier;


public readonly record struct UserId(Guid Value)
{
    public static UserId Empty => new(Guid.Empty);

    public static UserId New() => new(Guid.NewGuid());

    public static UserId FromGuid(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;

    public static explicit operator UserId(Guid value) => new(value);
}