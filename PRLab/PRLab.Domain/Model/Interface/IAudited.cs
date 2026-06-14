using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value;

namespace PRLab.Domain.Model.Interface;

public interface IAudited
{
    AuditInfo Audit { get; }

    void MarkUpdated(User? changedBy = null);
    
    void MarkDeleted(User? deletedBy = null);

}