using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Ownership;

namespace PRLab.Domain.Model.Interface;

public interface IOwnedData
{
    OwnershipInfo Ownership { get; }
}