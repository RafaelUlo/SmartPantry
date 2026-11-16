using System.ComponentModel;
using System.Windows.Input;

namespace SmartPantry.Mobile.Ui;

public sealed class AsyncAction(
    Func<object?, Task> run,
    Action<Exception> reportError,
    Predicate<object?>? enabled = null) : ICommand, INotifyPropertyChanged
{
    public event EventHandler? CanExecuteChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool IsRunning { get; private set; }

    public bool CanExecute(object? parameter) => !IsRunning && (enabled?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter) => await ExecuteAsync(parameter);

    public async Task ExecuteAsync(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        SetRunning(true);
        try
        {
            await run(parameter);
        }
        catch (Exception exception)
        {
            reportError(exception);
        }
        finally
        {
            SetRunning(false);
        }
    }

    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private void SetRunning(bool running)
    {
        IsRunning = running;
        PropertyChanged?.Invoke(this, new(nameof(IsRunning)));
        Refresh();
    }
}
