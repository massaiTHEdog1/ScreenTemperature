using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Entities;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Entities.KeyBindingActions;
using Path = System.IO.Path;

namespace ScreenTemperature;

public class DatabaseContext : DbContext
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DbSet<Profile> Profiles { get; set; }

    public DbSet<KeyBindingAction> KeyBindingActions { get; set; }
    public DbSet<ApplyProfileAction> ApplyProfileActions { get; set; }
    //public DbSet<DecreaseBrightnessBy> DecreaseBrightnessByActions { get; set; }
    //public DbSet<IncreaseBrightnessBy> IncreaseBrightnessByActions { get; set; }
    //public DbSet<SetBrightnessTo> SetBrightnessToActions { get; set; }

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
        options.UseSqlite($"Data Source={DbPath}")
            .LogTo(s => Console.WriteLine(s), LogLevel.Information)
            .EnableDetailedErrors(true)
            .EnableSensitiveDataLogging(true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyBindingAction>().UseTptMappingStrategy();
        modelBuilder.Entity<Configuration>().UseTptMappingStrategy();
        modelBuilder.Entity<ApplyProfileAction>().HasOne(p => p.Profile).WithMany(x => x.ApplyProfileActions).OnDelete(DeleteBehavior.NoAction);
    }
}