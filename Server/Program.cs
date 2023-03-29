using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Data;
using ScreenTemperature.Entities.KeyBindingActions;
using ScreenTemperature.Middlewares;
using ScreenTemperature.Services;
using System.Windows.Forms;

namespace ScreenTemperature;

internal class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region Services

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddScoped<IScreenService, ScreenService>();
        builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
        builder.Services.AddScoped<IOptionsService, OptionsService>();
        builder.Services.AddScoped<IKeyBindingService, KeyBindingService>();

        builder.Services.AddDbContext<DatabaseContext>();

        builder.Services.AddGraphQLServer()
            //.RegisterDbContext<DatabaseContext>()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            #region Add polymorphed classes -> We have to explicitly register the interface implementations
            .AddObjectType<ApplyConfiguration>()
            .AddObjectType<DecreaseBrightnessBy>()
            .AddObjectType<IncreaseBrightnessBy>()
            .AddObjectType<SetBrightnessTo>()
            #endregion
            .AddProjections()
            .AddFiltering();

        #endregion

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                databaseContext.Database.Migrate();

                var test = databaseContext.KeyBindings.Where(x => x.Id == 1).ToList();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }

        app.UseMiddleware<ExceptionMiddleware>();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            //app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseStaticFiles();

        app.UseAuthorization();

        app.MapGraphQL();

        app.Run();
    }
}