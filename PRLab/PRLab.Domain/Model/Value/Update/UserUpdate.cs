using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Domain.Model.Value.Update;

public class UserUpdate
{
    public string? Name { get; set; }
    public UserRole? Role { get; set; }
}