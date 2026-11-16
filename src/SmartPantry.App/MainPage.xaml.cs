using Microsoft.AspNetCore.Components.WebView.Maui;
using SmartPantry.Mobile.Components;

namespace SmartPantry.Mobile;

public partial class MainPage : ContentPage
{
    internal Func<bool>? HandleBack { get; set; }

    public MainPage()
    {
        InitializeComponent();
#if IOS
        Web.HandlerChanged += (_, _) =>
        {
            if (Web.Handler?.PlatformView is WebKit.WKWebView nativeWebView)
                nativeWebView.ScrollView.ContentInsetAdjustmentBehavior = UIKit.UIScrollViewContentInsetAdjustmentBehavior.Never;
        };
#endif
        Web.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(PantryHost),
            Parameters = new Dictionary<string, object?> { [nameof(PantryHost.Host)] = this }
        });
    }

    protected override bool OnBackButtonPressed() => HandleBack?.Invoke() == true || base.OnBackButtonPressed();
}
