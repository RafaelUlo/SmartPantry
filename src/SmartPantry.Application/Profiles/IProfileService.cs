using SmartPantry.Application.Dtos;
using SmartPantry.Domain.AggregateRoots.Profiles;

namespace SmartPantry.Application.Profiles;

public interface IProfileService
{
    Task<HouseholdProfileDto> LoadAsync(CancellationToken cancellationToken = default);
    Task<HouseholdProfileDto> SaveAsync(HouseholdProfile profile, CancellationToken cancellationToken = default);
    Task<HouseholdProfileDto> SetActiveAsync(Guid profileId, CancellationToken cancellationToken = default);
}

