# Application, persistence and UI orchestration

## Responsibilities

- SmartPantry.Domain: entities, audit contract and domain invariants.
- SmartPantry.Application: profile use cases, DTOs and ISmartPantryDbContext/ISmartPantryDbContextFactory contracts. Queries use EF Core through the interface without referencing SQLite.
- SmartPantry.Infrastructure.SqlLite: EF Core configurations, migrations, context factory and one-time JSON import.
- SmartPantry.App: Razor screens, native integration, dependency injection, UI state and automation identifiers. UI helpers live in Ui under SmartPantry.Mobile.Ui.

PantryHost owns navigation and the editing draft and calls IProfileService. Screens and dialogs receive data and callbacks; they do not access persistence. Desktop tests link the platform-independent UI state files they exercise without referencing the Android/iOS executable.

## Persistence

The database is FileSystem.AppDataDirectory/smartpantry.db. A factory creates and disposes a context per operation, avoiding a shared context across Blazor events. Initialization applies migrations and imports households.json once in a transaction; the source file remains unchanged. Invalid JSON rolls back and can be corrected before retrying. LegacyImportState is private infrastructure metadata containing only the import marker.

EF maps HouseholdProfile, Diet and Ingredient directly. Diet is a required relationship; allergies and exclusions use separate many-to-many associations. Removing a preference removes the association, not the shared ingredient. Exclusions are a set; display order is not persisted.

HouseholdProfile.IsDefault is the sole source of active selection. A filtered unique index allows at most one non-deleted default. Selection changes release the old default and set the new default inside a transaction. The first profile becomes default automatically. Loading repairs a missing default using the first visible profile.

DomainProfilePersistence copies existing profiles, restrictions and selection before dropping old tables. ProfileDefaultSelection transfers selection to IsDefault. Removed-association tombstone history is not migrated. Downgrade of these schema changes requires restoring a backup. SeedDietsAndAllergens seeds fixed IDs and audit dates and remaps existing catalogue associations without duplicate visible names.

## Audit and soft delete

BaseConfiguration<TEntity> applies to concrete tables rather than registering BaseEntity as an EF inheritance hierarchy. Column order is Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy, then business fields. Actor fields have maximum length 256. Authentication is not implemented: CreatedBy is empty and UpdatedBy/DeletedBy remain null.

All SaveChanges overloads stamp UTC dates. Added entities receive CreatedAt; modified entities receive UpdatedAt while preserving creation metadata. Remove becomes an update of DeletedAt and UpdatedAt. Global filters exclude deleted domain entities. Association tables hold only foreign keys. ExecuteDelete and raw DELETE bypass soft deletion. IgnoreQueryFilters is an explicit maintenance escape hatch. Pre-audit rows retain unknown creation times rather than invented historical dates.

## Validation and tooling

Run from the repository root:

```powershell
dotnet tool restore
dotnet ef migrations add MigrationName --project src/SmartPantry.Infrastructure.SqlLite
dotnet ef migrations has-pending-model-changes --project src/SmartPantry.Infrastructure.SqlLite
dotnet test tests/SmartPantry.Tests
```

The design-time factory uses smartpantry.design.db, separate from installed app data. Use migrations, not EnsureCreated, on managed databases. Android compilation is validated on Windows; iOS packaging and actual device behavior require Mac/device validation. NativeAOT is not configured or verified.

## Safe area and automation

MainPage and its background Grid use SafeAreaEdges=None to paint behind system bars. The nested ContentView uses SafeAreaEdges=SoftInput: the WebView extends behind system bars and cutouts, while keyboard insets remain enabled. CSS adds no duplicate safe-area inset. Step bars have a non-growing 5px height. Stable data-testid attributes support Katalon; native camera automation uses the native context rather than the WebView.

## References

- https://learn.microsoft.com/en-us/ef/core/querying/filters?tabs=ef10
- https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
- https://learn.microsoft.com/en-us/dotnet/maui/user-interface/safe-area?view=net-maui-10.0


## Catalogue options and ingredient search

ProfileDraft receives instance options queried through ICatalogueService. Diet options come from Diets; the allergy selector lists Ingredients without an allergen classification. No IsAllergen domain property exists. Exclusion search queries the database after a 250 ms debounce, ignores stale results, and displays matching ingredients. When no matches exist, an explicit Create and add button persists a new ingredient and selects it. Names are trimmed and creation reuses a case-insensitive existing match. Automation IDs include Exclusions.Input, Exclusions.Results, Exclusions.Option.{id}, Exclusions.NoMatches and Exclusions.Add.


## Blazor page navigation

PantryHost provides the shared profile draft, application callbacks and dialog state. Router/RouteView render the route components in Components/Pages: /welcome, /home, /profiles, /profiles/new, /profiles/{id}/edit and /setup/{number}. NavigationManager handles transitions within the WebView; Shell navigation remains for the native barcode scanner. The wizard keeps its bounded step switch inside SetupScreen rather than using it as the application router. Native Back dispatches to the Blazor renderer before changing page state. HTML boolean attributes use Razor expressions (disabled="@Busy"), so false removes the attribute instead of emitting a literal value.


Edge-to-edge configuration lives in MainPage.xaml: the page/background extend to the window edges while the content container applies only keyboard insets. Android disables decor fitting explicitly; iOS disables automatic WKWebView scroll insets. No fixed status-bar heights or CSS env insets are added. Android retains dark system icons against the light background. iOS uses native safe-area geometry; device validation still requires a Mac/iPhone.
