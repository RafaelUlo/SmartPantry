using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SmartPantry.Application.Catalogue;
using SmartPantry.Application.Dtos;
using SmartPantry.Application.Profiles;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Domain.Exceptions;
using SmartPantry.Mobile.Features.BarcodeScanning;
using SmartPantry.Mobile.Ui;

namespace SmartPantry.Mobile.Components;

public partial class PantryHost
{
    [Parameter, EditorRequired] public MainPage Host { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IProfileService Profiles { get; set; } = default!;
    [Inject] private ICatalogueService Catalogue { get; set; } = default!;
    private string[] diets = [], allergens = [];
    private void PopulateOptions() => draft.SetOptions(diets, allergens);
    internal enum Screen { Loading, Failure, Welcome, Setup, Home, Profiles, Editor }
    internal enum Sheet { None, Profiles, Allergies, Delete }
    private Screen screen = Screen.Loading;
    private Sheet sheet;
    internal HouseholdProfileDto book = HouseholdProfileDto.Empty;
    internal ProfileDraft draft = new();
    internal int step;
    internal bool creating, openingScanner, busy;
    internal string? error;

    protected override async Task OnInitializedAsync()
    {
        Host.HandleBack = () =>
        {
            if (!busy && sheet == Sheet.None && screen is Screen.Home or Screen.Welcome) return false;
            _ = InvokeAsync(BackRequested);
            return true;
        };
        Navigation.LocationChanged += LocationChanged;
        await LoadAsync();
    }
    public void Dispose()
    {
        Host.HandleBack = null;
        Navigation.LocationChanged -= LocationChanged;
    }

    internal Guid? EditingId { get; private set; }

    private void LocationChanged(object? sender, LocationChangedEventArgs args)
    {
        var path = Navigation.ToBaseRelativePath(args.Location).Split('?')[0].Trim('/');
        screen = path.StartsWith("setup/") ? Screen.Setup
            : path == "home" ? Screen.Home
            : path == "profiles" ? Screen.Profiles
            : path.StartsWith("profiles/") ? Screen.Editor : Screen.Welcome;
        if (screen == Screen.Setup && int.TryParse(path.Split('/').Last(), out var number)) step = Math.Clamp(number - 1, 0, 4);
        sheet = Sheet.None;
        error = null;
        _ = InvokeAsync(StateHasChanged);
    }

    private string RouteFor(Screen target) => target switch
    {
        Screen.Setup => $"setup/{step + 1}",
        Screen.Home => "home",
        Screen.Profiles => "profiles",
        Screen.Editor => creating ? "profiles/new" : $"profiles/{EditingId}/edit",
        _ => "welcome"
    };

    private async Task LoadAsync()
    {
        if (busy) return;
        busy = true; screen = Screen.Loading; error = null;
        try
        {
            book = await Profiles.LoadAsync();
            var options = await Catalogue.GetProfileOptionsAsync();
            diets = options.Diets.Select(d => d.Name).ToArray();
            allergens = options.Allergens.Select(a => a.Name).ToArray();
            PopulateOptions();
            screen = book.Profiles.Count == 0 ? Screen.Welcome : Screen.Home;
            Navigation.NavigateTo(RouteFor(screen), replace: true);
        }
        catch (Exception) { screen = Screen.Failure; }
        finally { busy = false; }
    }

    internal void Go(Screen target)
    {
        if (busy) return;
        if (target == Screen.Setup) step = 0;
        screen = target; sheet = Sheet.None; error = null;
        Navigation.NavigateTo(RouteFor(target));
    }
    internal void OpenSheet(Sheet target)
    {
        if (busy) return;
        sheet = target;
        // Routed pages call this host through a cascading parameter. Their events
        // rerender the page, so the host must refresh its own dialog outlet too.
        StateHasChanged();
    }
    private void CloseSheet() { if (!busy) sheet = Sheet.None; }
    internal void BackRequested() => Back();
    private bool Back()
    {
        if (busy) return true;
        if (sheet != Sheet.None) sheet = Sheet.None;
        else if (screen == Screen.Setup && step > 0) { step--; Navigation.NavigateTo(RouteFor(Screen.Setup)); }
        else if (screen == Screen.Setup) Go(Screen.Welcome);
        else if (screen == Screen.Editor) Go(Screen.Profiles);
        else if (screen == Screen.Profiles) Go(Screen.Home);
        else return false;
        error = null;
        _ = InvokeAsync(StateHasChanged);
        return true;
    }


    internal void Edit(HouseholdProfile profile)
    {
        if (busy) return;
        creating = false; EditingId = profile.Id; draft = new(profile); PopulateOptions(); Go(Screen.Editor);
    }
    internal void Create()
    {
        if (busy) return;
        creating = true; EditingId = null; draft = new(""); PopulateOptions(); Go(Screen.Editor);
    }
    internal void ToggleAllergen(string value)
    {
        if (value == "None") draft.Allergies.Clear();
        else if (!draft.Allergies.Add(value)) draft.Allergies.Remove(value);
    }

    internal async Task AddExclusion(string ingredient)
    {
        var value = ingredient.Trim();
        if (value.Length > 0 && !draft.Exclusions.Contains(value, StringComparer.OrdinalIgnoreCase))
            draft.Exclusions.Add(value);
        try
        {
            var options = await Catalogue.GetProfileOptionsAsync();
            diets = options.Diets.Select(d => d.Name).ToArray();
            allergens = options.Allergens.Select(i => i.Name).ToArray();
            PopulateOptions();
        }
        catch (Exception) { error = "Ingredient added, but the options could not be refreshed. Please reopen the profile."; }
    }

    internal void RemoveExclusion(string ingredient) => draft.Exclusions.Remove(ingredient);


    private bool Valid()
    {
        error = !draft.Name.Validate() ? draft.Name.Error : !draft.Servings.Validate() ? draft.Servings.Error : null;
        return error is null;
    }
    internal async Task NextAsync()
    {
        if (busy) return;
        if (step == 4) await SaveAsync();
        else if (Valid()) { step++; Navigation.NavigateTo(RouteFor(Screen.Setup)); }
    }
    internal async Task SaveAsync()
    {
        if (busy || !Valid()) return;
        busy = true;
        try
        {
            book = await Profiles.SaveAsync(draft.Build());
            screen = screen == Screen.Setup ? Screen.Home : Screen.Profiles;
            Navigation.NavigateTo(RouteFor(screen), replace: true);
            sheet = Sheet.None; error = null;
        }
        catch (DomainException exception) { error = exception.Message; }
        catch (Exception) { error = "Could not save the profile. Please try again."; }
        finally { busy = false; }
    }
    private async Task SelectProfileAsync(Guid profileId)
    {
        if (busy) return;
        busy = true;
        try
        {
            book = await Profiles.SetActiveAsync(profileId);
            screen = Screen.Home; sheet = Sheet.None; error = null;
            Navigation.NavigateTo(RouteFor(Screen.Home), replace: true);
        }
        catch (Exception) { error = "Could not switch the profile. Please try again."; }
        finally { busy = false; }
    }
    internal async Task Scan()
    {
        if (openingScanner) return;
        openingScanner = true;
        try { await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(nameof(BarcodeScannerPage))); }
        finally { openingScanner = false; }
    }
}

