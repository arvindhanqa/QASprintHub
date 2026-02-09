using System.Windows;

namespace QASprintHub.Views.Dialogs;

public partial class SetupWizardDialog : Window
{
    public SetupWizardDialog()
    {
        InitializeComponent();
    }

    public bool SetupCompleted { get; set; }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is ViewModels.Dialogs.SetupWizardViewModel vm)
        {
            SetupCompleted = vm.IsComplete;
        }
    }
}
