using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Commands;
using ScreenTemperature.Entities.Configurations;
using Path = System.IO.Path;

namespace ScreenTemperature;

public class DatabaseContext : DbContext
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DbSet<Command> Commands { get; set; }
    public DbSet<ApplyConfigurationCommand> ApplyConfigurationCommands { get; set; }
    //public DbSet<DecreaseBrightnessBy> DecreaseBrightnessByCommands { get; set; }
    //public DbSet<IncreaseBrightnessBy> IncreaseBrightnessByCommands { get; set; }
    //public DbSet<SetBrightnessTo> SetBrightnessToCommands { get; set; }

    public DbSet<KeyBinding> KeyBindings { get; set; }

    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<TemperatureConfiguration> TemperatureConfigurations { get; set; }
    public DbSet<ColorConfiguration> ColorConfigurations { get; set; }

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

        if (_webHostEnvironment.IsDevelopment())
        {
            options.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableDetailedErrors(true)
            .EnableSensitiveDataLogging(true);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Command>().UseTptMappingStrategy(); // for polymorphism
        modelBuilder.Entity<Configuration>().UseTptMappingStrategy(); // for polymorphism
    }
}