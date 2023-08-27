using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Entities.KeyBindingActions;
using ScreenTemperature.Middlewares;
using ScreenTemperature.Services;
using System.Text.Json;
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
        builder.Services.AddScoped<IProfileService, ProfileService>();
        builder.Services.AddScoped<IOptionsService, OptionsService>();
        builder.Services.AddScoped<IKeyBindingService, KeyBindingService>();

        builder.Services.AddDbContext<DatabaseContext>();

        builder.Services.AddFastEndpoints();

        #endregion

        if (builder.Environment.IsDevelopment())
        { 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "devCORS",
                                  policy =>
                                  {
                                      policy.WithOrigins("http://localhost:5173");
                                  });
            });
        }

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

        //app.UseStaticFiles();

        app.UseAuthorization();

        app.UseFastEndpoints(c => {
            // For PascalCase
            c.Serializer.Options.PropertyNamingPolicy = null;
        });

        if (builder.Environment.IsDevelopment())
        {
            app.UseCors("devCORS");
        }

        app.Run();
    }
}