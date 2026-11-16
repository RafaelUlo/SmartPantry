# SmartPantry mobile app

Edit household screens in `Components/Kitchen.razor`, reusable controls in `Components`, and visual styles in `wwwroot/app.css`. The MAUI host runs the Razor UI locally on Android (API 24+) and iOS. The in-app camera preview uses the native barcode scanner page.

## Barcode scanner POC

Open SmartPantry.sln, select SmartPantry.App as the startup project and deploy to a camera-equipped Android phone or iPhone. iPhone deployment requires a Mac with Xcode and device signing.

1. From **My kitchen**, select **Scan a product barcode**. Tap **Start camera**, grant permission and point the rear camera at a retail barcode.
2. Compare the displayed format and digits with the package. The preview should close after one result.
3. Tap **Scan again** to read another package.
4. Background the app while scanning. On return, restart the camera with the scan button.
5. Revoke camera permission and retry to exercise denial. Follow **Open camera settings**, grant permission and retry.

Frames are processed on-device and the scan result stays in memory.

