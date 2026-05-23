using PRLab.Value;

namespace PRLab.Model.Interface;

public interface IAudited
{
    public AuditInfo Audit { get; }
}