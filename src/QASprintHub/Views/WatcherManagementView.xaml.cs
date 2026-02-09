using QASprintHub.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace QASprintHub.Views;

public partial class WatcherManagementView : UserControl
{
    public WatcherManagementView()
    {
        InitializeComponent();
    }

    private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is WatcherManagementViewModel vm && vm.SelectedMember != null)
        {
            vm.EditMemberCommand.Execute(vm.SelectedMember);
        }
    }
}
