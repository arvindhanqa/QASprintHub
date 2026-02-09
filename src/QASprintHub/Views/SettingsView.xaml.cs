using QASprintHub.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace QASprintHub.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        Loaded += SettingsView_Loaded;
    }

    private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            await vm.LoadSettingsAsync();
        }
    }
}
