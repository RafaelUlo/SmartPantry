using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SmartPantry.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if (Window is { } window)
        {
            AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(window, false);
            if (AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView) is { } bars)
            {
                bars.AppearanceLightStatusBars = true;
                bars.AppearanceLightNavigationBars = true;
            }
        }
    }
}
