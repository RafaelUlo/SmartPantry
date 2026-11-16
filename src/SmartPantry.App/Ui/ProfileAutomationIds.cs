namespace SmartPantry.Mobile.Ui;

public static class ProfileAutomationIds
{
    public const string StartOnboarding = "Welcome.StartOnboarding";
    public const string HouseholdName = "Onboarding.HouseholdName";
    public const string HouseholdNameError = "Onboarding.HouseholdName.Error";
    public const string ServingSize = "Onboarding.ServingSize";
    public const string CompleteOnboarding = "Onboarding.Complete";
    public const string SavePreferences = "DietaryPreferences.Save";
    public const string SaveProfile = "ProfileCreation.Save";
    public const string OpenSelector = "AppHeader.OpenProfileSelector";
    public const string ActiveProfile = "AppHeader.ActiveProfile";

    public static string ProfileOption(string persistedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(persistedKey);
        return $"ProfileSelector.Option.{Uri.EscapeDataString(persistedKey)}";
    }
}
