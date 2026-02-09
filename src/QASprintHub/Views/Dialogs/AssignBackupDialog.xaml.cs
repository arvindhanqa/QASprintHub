using QASprintHub.Models;
using System.Collections.Generic;
using System.Windows;

namespace QASprintHub.Views.Dialogs;

public partial class AssignBackupDialog : Window
{
    public AssignBackupDialog(List<TeamMember> availableMembers)
    {
        InitializeComponent();
        MembersListBox.ItemsSource = availableMembers;
    }

    public TeamMember? SelectedMember => MembersListBox.SelectedItem as TeamMember;
    public bool WasConfirmed { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedMember == null)
        {
            MessageBox.Show("Please select a backup watcher.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        WasConfirmed = true;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        WasConfirmed = false;
        DialogResult = false;
        Close();
    }
}
