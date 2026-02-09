using System.Threading.Tasks;

namespace QASprintHub.Services;

public interface IExportService
{
    Task ExportToExcelAsync(string filePath);
    Task BackupDatabaseAsync(string filePath);
    Task RestoreDatabaseAsync(string filePath);
}
