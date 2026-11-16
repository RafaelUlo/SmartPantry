namespace SmartPantry.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(Features.BarcodeScanning.BarcodeScannerPage), typeof(Features.BarcodeScanning.BarcodeScannerPage));
	}
}

