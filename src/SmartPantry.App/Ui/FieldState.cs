using System.ComponentModel;

namespace SmartPantry.Mobile.Ui;

public sealed class FieldState<T>(T initialValue, Func<T, string?> validator) : INotifyPropertyChanged
{
    private T value = initialValue;
    private bool validationRequested;

    public event PropertyChangedEventHandler? PropertyChanged;
    public string? Error { get; private set; }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public T Value
    {
        get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
            this.value = value;
            PropertyChanged?.Invoke(this, new(nameof(Value)));
            if (validationRequested) Validate();
        }
    }

    public bool Validate()
    {
        validationRequested = true;
        var error = validator(value);
        if (Error != error)
        {
            Error = error;
            PropertyChanged?.Invoke(this, new(nameof(Error)));
            PropertyChanged?.Invoke(this, new(nameof(HasError)));
        }
        return !HasError;
    }
}
