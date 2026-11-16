# Onboarding component state

`PantryHost` owns the `ProfileDraft` for the current edit session. Step components receive only the values they display and send changes back with `EventCallback<T>`. They do not create another draft or save profiles.

For example, the parent binds its existing name field:

```razor
<HouseholdConfig @bind-Value="draft.Name.Value" />
```

The child declares the binding contract:

```csharp
[Parameter, EditorRequired] public string Value { get; set; } = string.Empty;
[Parameter] public EventCallback<string> ValueChanged { get; set; }
```

Its input forwards each edit without assigning to the parameter:

```razor
<input @bind:get="Value" @bind:set="ValueChanged.InvokeAsync" @bind:event="oninput" />
```

`@bind-Value` expands to passing `Value` and handling `ValueChanged` in the parent. That callback updates `draft.Name.Value` and makes Blazor rerender the parent. There is no need for a global singleton, a second draft, or manual `StateHasChanged` calls for these UI events.

`SetupScreen` coordinates the steps using the form model owned by `PantryHost`.

The five steps are:

- `HouseholdConfig`: name through `Value`/`ValueChanged`.
- `ServingsConfig`: bounded serving count through `Value`/`ValueChanged`; the parent's `Count` property adapts the draft's string field to an integer.
- `DietConfig`: diet through `Value`/`ValueChanged`.
- `AllergiesConfig`: read-only selected values and a `Changed` callback; the parent applies the mutually exclusive None rule.
- `ExclusionsConfig`: summary values, read-only ingredients, and `Added`/`Removed` callbacks forwarded to the parent.

`Exclusions` keeps only temporary input text and its input error locally. The profile's ingredient list belongs to `PantryHost`. The editor uses the same callbacks as onboarding.

Because the section has `@key="(screen, step)"`, Blazor recreates the child when changing steps. The parent remains alive and retains the draft, so Back/Continue do not discard profile values. Parent validation and persistence still run before saving. Existing `data-testid` selectors remain on the actual HTML controls for Katalon.

Microsoft documentation: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/data-binding?view=aspnetcore-10.0
