# EP-01 presentation foundation

The UI and its state helpers live in SmartPantry.App. `Ui` contains ProfileDraft, FieldState, AsyncAction and ProfileAutomationIds under the SmartPantry.Mobile.Ui namespace. Desktop tests link the platform-independent state files they exercise; there is no separate Presentation project.

Bind an editable control two-way to FieldState.Value. Call Validate on submission to reveal errors, then edits revalidate automatically. Bind the feedback label to Error and HasError. Validators belong to the feature and supply localized text.

Bind buttons to AsyncAction and their progress indicator to its IsRunning property. Construct and execute commands on the UI thread. Supply the feature's error handler to publish an observable failure message. Call Refresh when an external condition changes whether the action is enabled.

Expose EP-01 automation identifiers as data-testid attributes on interactive Razor controls, validation messages, screen containers and dialogs. Use CSS selectors such as [data-testid="Editor.Name"], [data-testid="ProfileCreation.Save"] and [data-testid="Allergy.Eggs"][aria-pressed="true"] in the step definitions. The Android debug build enables WebView debugging for automation. In Katalon, locate these elements in the WebView context and switch to the native context for the camera page. For repeated profiles, derive the row ID from the persisted profile key. Keep identifiers independent of translated captions, list order and editable names.

The welcome, household and servings illustrations come from the supplied mockups. The implementation uses responsive CSS and semantic HTML form controls. The exclusions search offers a small local suggestion list and accepts custom ingredients. Profile edits and the active profile are saved together in the app data directory. The camera proof of concept is accessible from the kitchen after onboarding.

The proposed limits of 50 name characters and 20 servings are used for onboarding and editing. The delete action opens a prompt pending the separate deletion story. On-device screenshot comparison, live camera scans and Katalon execution are pending.


