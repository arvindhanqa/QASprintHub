using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace QASprintHub.Views.Dialogs;

public partial class InputDialog : Window, INotifyPropertyChanged
{
    private string _title = "Input";
    private string _prompt = "Enter value:";
    private string _inputValue = "";

    public InputDialog()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += (s, e) => InputTextBox.Focus();
    }

    public InputDialog(string title, string prompt, string defaultValue = "") : this()
    {
        Title = title;
        Prompt = prompt;
        InputValue = defaultValue;
    }

    public new string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public string Prompt
    {
        get => _prompt;
        set { _prompt = value; OnPropertyChanged(); }
    }

    public string InputValue
    {
        get => _inputValue;
        set { _inputValue = value; OnPropertyChanged(); }
    }

    public bool WasConfirmed { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        WasConfirmed = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        WasConfirmed = false;
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
