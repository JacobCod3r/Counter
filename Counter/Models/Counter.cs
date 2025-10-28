using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Counter.Models;

public class Counter : INotifyPropertyChanged
{
    private long _value;

    public string Name { get; set; } = "Counter";
    public long InitialValue { get; set; } = 0;
    public string ColorName { get; set; } = "Blue";
    public string ColorHex { get; set; } = "#CFE8FF";

    public long Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
