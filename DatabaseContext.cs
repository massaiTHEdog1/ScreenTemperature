using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Entities;

namespace ScreenTemperature;

public class DatabaseContext : DbContext
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DbSet<Configuration> Configurations { get; set; }

    public string DbPath { get; }

    private string DatabasePath
    {
        get
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                return "database.db";
            }
            else
            {
                var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                return Path.Join(localAppDataPath, "ScreenTemperature", "database.db");
            }
        }
    }

    public DatabaseContext(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;

        var databaseDirectoryPath = Path.GetDirectoryName(DatabasePath);

        if (!string.IsNullOrWhiteSpace(databaseDirectoryPath) && !Directory.Exists(databaseDirectoryPath))
        {
            Directory.CreateDirectory(databaseDirectoryPath);
        }

        DbPath = DatabasePath;
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}