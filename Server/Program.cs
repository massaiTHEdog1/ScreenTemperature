using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Hubs;
using ScreenTemperature.Middlewares;
using ScreenTemperature.Services;
using Vernou.Swashbuckle.HttpResultsAdapter;

namespace ScreenTemperature;

internal class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region Services

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddSingleton<IScreenService, ScreenService>();
        builder.Services.AddScoped<IOptionsService, OptionsService>();

        builder.Services.AddDbContext<DatabaseContext>();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.OperationFilter<HttpResultsOperationFilter>();
                c.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
                c.UseOneOfForPolymorphism();
            });
        }

        builder.Services.AddSignalR();
        builder.Services.AddSingleton<HotKeyManager>();

        #endregion

        if (builder.Environment.IsDevelopment())
        { 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "devCORS",
                                  policy =>
                                  {
                                      policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                                      //policy.WithMethods()
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

                var hotKeyManager = scope.ServiceProvider.GetRequiredService<HotKeyManager>();

                _ = hotKeyManager.InitAsync();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }

        Task.Run(async () =>
        {
            var screenService = app.Services.GetService<IScreenService>();
            await screenService.GetScreensAsync();// load screens in memory
        });

        app.UseMiddleware<ExceptionMiddleware>();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            //app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseStaticFiles();

        if (builder.Environment.IsDevelopment())
        {
            app.UseCors("devCORS");
        }

        app.UseAuthorization();

        if (builder.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.MapHub<Hub>("/hub");

        app.Run();
    }
}