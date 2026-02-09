using System;
using System.Windows;
using System.ComponentModel;

namespace QASprintHub.Views.Dialogs;

public partial class SetupWizardDialog : Window
{
    public SetupWizardDialog()
    {
        InitializeComponent();
        this.Loaded += SetupWizardDialog_Loaded;
        this.DataContextChanged += SetupWizardDialog_DataContextChanged;
        this.Closing += Window_Closing;
    }

    public bool SetupCompleted { get; set; }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is ViewModels.Dialogs.SetupWizardViewModel vm)
        {
            SetupCompleted = vm.IsComplete;
        }
    }

    private void SetupWizardDialog_Loaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Dialogs.SetupWizardViewModel vm)
        {
            // Set AlternationCount to team size so ItemsControl.AlternationIndex works
            try
            {
                MembersList.AlternationCount = Math.Max(1, vm.TeamSize);
            }
            catch
            {
                // ignore
            }
            // If the VM is already complete, close immediately
            if (vm.IsComplete)
            {
                Close();
            }
            // Listen for completion changes
            vm.PropertyChanged += Vm_PropertyChanged;
        }
    }

    private void SetupWizardDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ViewModels.Dialogs.SetupWizardViewModel oldVm)
        {
            oldVm.PropertyChanged -= Vm_PropertyChanged;
        }

        if (e.NewValue is ViewModels.Dialogs.SetupWizardViewModel newVm)
        {
            newVm.PropertyChanged += Vm_PropertyChanged;
            if (newVm.IsComplete)
            {
                Close();
            }
        }
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.Dialogs.SetupWizardViewModel.IsComplete) && sender is ViewModels.Dialogs.SetupWizardViewModel vm)
        {
            if (vm.IsComplete)
            {
                // Close the dialog when VM marks setup complete
                Dispatcher.Invoke(() => Close());
            }
        }
    }
}
