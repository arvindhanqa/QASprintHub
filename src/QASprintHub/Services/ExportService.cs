using QASprintHub.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public class ExportService : IExportService
{
    private readonly AppDbContext _context;

    public ExportService(AppDbContext context)
    {
        _context = context;
    }

    public Task ExportToExcelAsync(string filePath)
    {
        // TODO: Implement Excel export using a library like EPPlus or ClosedXML
        throw new NotImplementedException("Excel export will be implemented in a future update.");
    }

    public async Task BackupDatabaseAsync(string filePath)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(appDataPath, "QASprintHub", "qasprinthub.db");

        if (File.Exists(dbPath))
        {
            await Task.Run(() => File.Copy(dbPath, filePath, overwrite: true));
        }
        else
        {
            throw new FileNotFoundException("Database file not found.");
        }
    }

    public async Task RestoreDatabaseAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Backup file not found.");
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(appDataPath, "QASprintHub", "qasprinthub.db");

        await Task.Run(() => File.Copy(filePath, dbPath, overwrite: true));

        // Reload the database context
        await _context.Database.EnsureCreatedAsync();
    }
}
