using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ScreenTemperature.Middlewares;
using ScreenTemperature.Services;

namespace ScreenTemperature;

internal class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region Services

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddScoped<IScreensService, ScreensService>();
        builder.Services.AddScoped<IConfigurationsService, ConfigurationsService>();
        builder.Services.AddScoped<IOptionsService, OptionsService>();

        builder.Services.AddDbContext<DatabaseContext>();

        #endregion

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                databaseContext.Database.Migrate();
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

        app.UseHttpsRedirection();
        //app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute("default", "{controller}/{action}");

        app.Run();
    }
}