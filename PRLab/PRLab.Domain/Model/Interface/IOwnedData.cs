using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Ownership;

namespace PRLab.Domain.Model.Interface;

public interface IOwnedData
{
    OwnershipInfo Ownership { get; }
}