using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System.Windows;
using System.Windows.Controls;

namespace QASprintHub.Views.Dialogs;

public partial class EditPRDialog : Window
{
    private readonly SprintPR _pr;

    public EditPRDialog(SprintPR pr)
    {
        InitializeComponent();
        _pr = pr;

        // Pre-populate fields with existing PR data
        TitleTextBox.Text = pr.Title;
        AuthorTextBox.Text = pr.Author ?? string.Empty;
        LinkTextBox.Text = pr.Link ?? string.Empty;

        // Set status ComboBox selection
        StatusComboBox.SelectedIndex = pr.Status switch
        {
            PRStatus.Pending => 0,
            PRStatus.Merged => 1,
            PRStatus.Blocked => 2,
            PRStatus.Closed => 3,
            _ => 0
        };

        // Set priority ComboBox selection
        PriorityComboBox.SelectedIndex = pr.Priority switch
        {
            PRPriority.Low => 0,
            PRPriority.Medium => 1,
            PRPriority.High => 2,
            _ => 1
        };

        Loaded += (s, e) => TitleTextBox.Focus();
    }

    public string PRTitle => TitleTextBox.Text;
    public string? Author => string.IsNullOrWhiteSpace(AuthorTextBox.Text) ? null : AuthorTextBox.Text;
    public string? Link => string.IsNullOrWhiteSpace(LinkTextBox.Text) ? null : LinkTextBox.Text;

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
