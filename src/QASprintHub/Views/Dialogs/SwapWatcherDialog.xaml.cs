using QASprintHub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace QASprintHub.Views.Dialogs;

public partial class SwapWatcherDialog : Window
{
    public SwapWatcherDialog(string currentWatcherName, List<TeamMember> availableMembers)
    {
        InitializeComponent();
        CurrentWatcherText.Text = currentWatcherName;
        MembersListBox.ItemsSource = availableMembers;
    }

    public TeamMember? SelectedMember => MembersListBox.SelectedItem as TeamMember;
    public string Reason => ReasonTextBox.Text;
    public bool WasConfirmed { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedMember == null)
        {
            MessageBox.Show("Please select a team member to swap to.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
