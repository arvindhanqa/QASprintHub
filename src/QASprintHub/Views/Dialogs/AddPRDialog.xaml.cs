using QASprintHub.Models.Enums;
using System.Windows;
using System.Windows.Controls;

namespace QASprintHub.Views.Dialogs;

public partial class AddPRDialog : Window
{
    public AddPRDialog()
    {
        InitializeComponent();
        Loaded += (s, e) => TitleTextBox.Focus();
    }

    public string PRTitle => TitleTextBox.Text;
    public string Author => AuthorTextBox.Text;

    public PRStatus Status
    {
        get
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                return tag switch
                {
                    "Pending" => PRStatus.Pending,
                    "Merged" => PRStatus.Merged,
                    "Blocked" => PRStatus.Blocked,
                    "Closed" => PRStatus.Closed,
                    _ => PRStatus.Pending
                };
            }
            return PRStatus.Pending;
        }
    }

    public PRPriority Priority
    {
        get
        {
            if (PriorityComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                return tag switch
                {
                    "Low" => PRPriority.Low,
                    "Medium" => PRPriority.Medium,
                    "High" => PRPriority.High,
                    _ => PRPriority.Medium
                };
            }
            return PRPriority.Medium;
        }
    }

    public bool WasConfirmed { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            MessageBox.Show("Please enter a PR title.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        WasConfirmed = true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        WasConfirmed = false;
        DialogResult = false;
    }
}
