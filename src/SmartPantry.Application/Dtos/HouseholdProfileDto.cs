using SmartPantry.Domain.AggregateRoots.Profiles;

namespace SmartPantry.Application.Dtos;

public sealed record HouseholdProfileDto(Guid ActiveId, List<HouseholdProfile> Profiles)
{
    public static HouseholdProfileDto Empty => new(Guid.Empty, []);
    public HouseholdProfile Active => Profiles.First(p => p.Id == ActiveId);
}