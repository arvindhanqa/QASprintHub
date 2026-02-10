using QASprintHub.Models;
using QASprintHub.ViewModels;
using System.Windows.Controls;

namespace QASprintHub.Views;

public partial class CalendarDiaryView : UserControl
{
    public CalendarDiaryView()
    {
        InitializeComponent();
    }

    private async void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            // Get the PR being edited
            if (e.Row.Item is SprintPR pr)
            {
                // Delay to allow the binding to update
                await System.Threading.Tasks.Task.Delay(100);

                // Get the PRService and update the PR
                var prService = App.GetService<Services.IPRService>();
                await prService.UpdatePRAsync(pr);

                // Reload the view to reflect changes
                if (DataContext is CalendarDiaryViewModel viewModel)
                {
                    await viewModel.LoadDataAsync();
                }
            }
        }
    }
}
