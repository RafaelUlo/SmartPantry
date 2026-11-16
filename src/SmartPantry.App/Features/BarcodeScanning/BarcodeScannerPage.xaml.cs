using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace SmartPantry.Mobile.Features.BarcodeScanning;

public partial class BarcodeScannerPage : ContentPage
{
    private CameraBarcodeReaderView? camera;
    private int session;

    private Window? observedWindow;

    public BarcodeScannerPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            observedWindow = Window;
            if (observedWindow is not null) observedWindow.Stopped += OnStopped;
        };
        Unloaded += (_, _) =>
        {
            if (observedWindow is not null) observedWindow.Stopped -= OnStopped;
            observedWindow = null;
            StopCamera();
        };
    }

    protected override void OnDisappearing()
    {
        StopCamera();
        base.OnDisappearing();
    }
    private void OnStopped(object? sender, EventArgs args) => StopCamera();

    private async void StartScan(object? sender, EventArgs args)
    {
        var request = ++session;
        Scan.IsEnabled = false;
        Settings.IsVisible = false;
        try
        {
            if (!ZXing.Net.Maui.BarcodeScanning.IsSupported)
            {
                Status.Text = "No supported camera was found.";
                return;
            }

            var permission = await Permissions.RequestAsync<Permissions.Camera>();
            if (request != session) return;
            if (permission != PermissionStatus.Granted)
            {
                Status.Text = "Camera permission is needed to scan.";
                Settings.IsVisible = true;
                return;
            }

            camera = new CameraBarcodeReaderView
            {
                AutomationId = "Scanner.Preview",
                CameraLocation = CameraLocation.Rear,
                Options = new BarcodeReaderOptions { Formats = BarcodeFormats.OneDimensional, AutoRotate = true }
            };
            camera.BarcodesDetected += OnDetected;
            Preview.Content = camera;
            Status.Text = "Point the rear camera at the barcode.";
        }
        catch (Exception ex)
        {
            StopCamera();
            Status.Text = $"Could not open the camera. {ex.Message}";
        }
        finally
        {
            Scan.IsEnabled = camera is null;
        }
    }

    private void OnDetected(object? sender, BarcodeDetectionEventArgs args)
    {
        var result = args.Results.FirstOrDefault();
        if (result is null) return;
        Dispatcher.Dispatch(() =>
        {
            if (!ReferenceEquals(sender, camera)) return;
            StopCamera();
            Status.Text = $"{result.Format}: {result.Value}";
            Scan.Text = "Scan again";
            SemanticScreenReader.Announce(Status.Text);
        });
    }

    private void StopCamera()
    {
        session++;
        if (camera is not null)
        {
            camera.BarcodesDetected -= OnDetected;
            camera.IsDetecting = false;
            var previous = camera;
            camera = null;
            previous.Handler?.DisconnectHandler();
            Preview.Content = null;
            Status.Text = "Camera stopped.";
        }
        Scan.IsEnabled = true;
    }

    private void OpenSettings(object? sender, EventArgs args) => AppInfo.ShowSettingsUI();
}



