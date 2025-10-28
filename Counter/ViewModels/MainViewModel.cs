using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using ModelCounter = Counter.Models.Counter;

namespace Counter.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    string _newName = string.Empty;
    string _newInitialText = "0";
    string _newColorName = "Blue";

    public string NewName
    {
        get => _newName;
        set {
            if (_newName == value)
            {
                return;
            }
            _newName = value; 
            OnPropertyChanged(); 
        }
    }

    public string NewInitialText
    {
        get => _newInitialText;
        set {
            if (_newInitialText == value)
            {
                return;
            }
            _newInitialText = value; 
            OnPropertyChanged(); 
        }
    }

    public string NewColorName
    {
        get => _newColorName;
        set {
            if (_newColorName == value)
            {
                return;
            }
            _newColorName = value; 
            OnPropertyChanged(); 
        }
    }

    public IReadOnlyList<string> AvailableColors { get; } = new[]
    {
        "Blue","Red","Green","Yellow","Purple","Orange","Teal","Pink","Gray","Indigo"
    };

    readonly Dictionary<string, string> _colorMap = new()
    {
        ["Blue"] = "#1565C0",
        ["Red"] = "#C62828",
        ["Green"] = "#2E7D32",
        ["Yellow"] = "#F9A825",
        ["Purple"] = "#6A1B9A",
        ["Orange"] = "#EF6C00",
        ["Teal"] = "#00695C",
        ["Pink"] = "#AD1457",
        ["Gray"] = "#455A64",
        ["Indigo"] = "#283593",
    };

    public ObservableCollection<ModelCounter> Counters { get; } = new();

    public ICommand AddCommand { get; }
    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand DeleteCommand { get; }

    string SavePath => Path.Combine(FileSystem.AppDataDirectory, "counters.json");

    public MainViewModel()
    {
        Counters.CollectionChanged += (_, _) =>
        {
            _ = SaveAsync();
        };

        AddCommand = new Command(AddCounter);

        IncrementCommand = new Command<ModelCounter>(c =>
        {
            if (c == null) 
            { 
                return; 
            }
            c.Value++;
        });

        DecrementCommand = new Command<ModelCounter>(c =>
        {
            if (c == null)
            {
                return;
            }
            c.Value--;
        });

        ResetCommand = new Command<ModelCounter>(c =>
        {
            if (c == null)
            {
                return;
            }
            c.Value = c.InitialValue;
        });

        DeleteCommand = new Command<ModelCounter>(c =>
        {
            if (c == null)
            {
                return;
            }
            c.PropertyChanged -= Item_PropertyChanged;
            Counters.Remove(c);
        });

        _ = LoadAsync();
    }

    void AddCounter()
    {
        if (!long.TryParse(NewInitialText, out var init))
        {
            init = 0;
        }
        var name = string.IsNullOrWhiteSpace(NewName) ? "Counter" : NewName.Trim();

        var colorName = string.IsNullOrWhiteSpace(NewColorName) ? "Blue" : NewColorName;
        var colorHex = _colorMap.TryGetValue(colorName, out var hex) ? hex : _colorMap["Blue"];

        var c = new ModelCounter
        {
            Name = name,
            InitialValue = init,
            Value = init,
            ColorName = colorName,
            ColorHex = colorHex
        };

        c.PropertyChanged += Item_PropertyChanged;
        Counters.Add(c);

        NewName = string.Empty;
        NewInitialText = "0";
        NewColorName = "Blue";
    }

    void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) 
    { 
        _ = SaveAsync(); 
    }

    async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                return;
            }

            await using var fs = File.OpenRead(SavePath);
            var list = await JsonSerializer.DeserializeAsync<List<ModelCounter>>(fs) ?? new List<ModelCounter>();

            Counters.Clear();
            foreach (var c in list)
            {
                if (string.IsNullOrWhiteSpace(c.ColorName))
                {
                    c.ColorName = "Blue";
                }
                if (string.IsNullOrWhiteSpace(c.ColorHex))
                {
                    c.ColorHex = _colorMap.TryGetValue(c.ColorName, out var hex) ? hex : _colorMap["Blue"];
                }

                c.PropertyChanged += Item_PropertyChanged;
                Counters.Add(c);
            }
        }
        catch
        {}
    }

    async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Counters, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
        }
        catch
        {}
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
