namespace SmartPantry.Domain.AggregateRoots.Profiles;

public static class ProfileRules
{
    public const int MaxNameLength = 50;
    public const int MinServings = 1;
    public const int MaxServings = 20;
    public static string? NameError(string? value) => value?.Trim().Length is >= 1 and <= MaxNameLength ? null : "Enter a name between 1 and 50 characters.";
    public static string? ServingsError(int value) => value is >= MinServings and <= MaxServings ? null : "Enter a whole number from 1 to 20.";
}