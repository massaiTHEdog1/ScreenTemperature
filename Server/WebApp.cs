using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Hubs;
using ScreenTemperature.Middlewares;
using ScreenTemperature.Services;
using System.Diagnostics;
using Vernou.Swashbuckle.HttpResultsAdapter;

namespace ScreenTemperature
{
    public class WebApp
    {
        private static WebApp _instance;
        public static WebApp Instance 
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WebApp();
                }

                return _instance;
            }
        }

        WebApp()
        {
            // private constructor
        }

        public WebApplication WebApplication { get; private set; }
        public Task Task { get; private set; }

        public void Start(CancellationToken cancellationToken)
        {
            var builder = WebApplication.CreateBuilder();

            #region Services

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddSingleton<IScreenService, ScreenService>();
            builder.Services.AddScoped<IParametersService, ParametersService>();

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

            #endregion


            WebApplication = builder.Build();

            using (var scope = WebApplication.Services.CreateScope())
            {
                try
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    databaseContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApp>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }

            Task.Run(async () =>
            {
                #region Load screens in memory

                var screenService = WebApplication.Services.GetService<IScreenService>();
                await screenService.GetScreensAsync();// load screens in memory

                #endregion

                #region Register HotKeys

                using (var scope = WebApplication.Services.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    var keyBindings = await databaseContext.KeyBindings.ToListAsync();

                    foreach (var keyBinding in keyBindings)
                    {
                        _ = HotKeyManager.Instance.RegisterHotKeyAsync(keyBinding.KeyCode, keyBinding.Alt, keyBinding.Control, false);
                        HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
                    }
                }

                #endregion

                #region Apply configurations

                using (var scope = WebApplication.Services.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    var configurations = await databaseContext.Configurations.ToListAsync();

                    foreach (var config in configurations)
                    {
                        if(config.ApplyAtStartup)
                        {
                            if(config.ApplyBrightness)
                                await screenService.ApplyBrightnessToScreenAsync(config.Brightness, config.DevicePath);

                            if(config is TemperatureConfiguration temperatureConfiguration && temperatureConfiguration.ApplyIntensity)
                                await screenService.ApplyKelvinToScreenAsync(temperatureConfiguration.Intensity, temperatureConfiguration.DevicePath);

                            if (config is ColorConfiguration colorConfiguration && colorConfiguration.ApplyColor)
                                await screenService.ApplyColorToScreenAsync(colorConfiguration.Color, colorConfiguration.DevicePath);
                        }
                    }
                }

                #endregion
            });

            WebApplication.UseMiddleware<ExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (!WebApplication.Environment.IsDevelopment())
            {
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                WebApplication.UseHsts();
            }

            WebApplication.UseDefaultFiles();
            WebApplication.UseStaticFiles();

            if (builder.Environment.IsDevelopment())
            {
                WebApplication.UseCors("devCORS");
            }

            //WebApplication.UseAuthorization();

            if (builder.Environment.IsDevelopment())
            {
                WebApplication.UseSwagger();
                WebApplication.UseSwaggerUI();
            }

            WebApplication.MapControllers();

            WebApplication.MapHub<Hub>("/hub");

            WebApplication.Urls.Add("https://localhost:61983");

            Task = WebApplication.RunAsync(cancellationToken);
        }

        private async void HotKeyManager_HotKeyPressed(object sender, HotKeyPressedEventArgs e)
        {
            using (var scope = WebApplication.Services.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var screenService = scope.ServiceProvider.GetRequiredService<IScreenService>();

                var matchingBinding = databaseContext.KeyBindings.Include(x => x.Configurations).SingleOrDefault(x => x.KeyCode == e.KeyCode && x.Alt == e.Alt && x.Control == e.Ctrl);

                if (matchingBinding != null)
                {
                    foreach (var config in matchingBinding.Configurations)
                    {
                        var result = false;

                        if (config.ApplyBrightness)
                        {
                            try
                            {
                                result = await screenService.ApplyBrightnessToScreenAsync(config.Brightness, config.DevicePath);
                            }
                            catch (Exception ex) { }

                            //await Clients.All.SendAsync("ApplyTemperatureResult", result);

                        }

                        if (config is ColorConfiguration colorConfiguration)
                        {
                            if (colorConfiguration.ApplyColor)
                            {
                                result = false;

                                try
                                {
                                    result = await screenService.ApplyColorToScreenAsync(colorConfiguration.Color, config.DevicePath);
                                }
                                catch (Exception ex) { }
                            }
                        }
                        else if (config is TemperatureConfiguration temperatureConfiguration)
                        {
                            if (temperatureConfiguration.ApplyIntensity)
                            {
                                result = false;

                                try
                                {
                                    result = await screenService.ApplyKelvinToScreenAsync(temperatureConfiguration.Intensity, config.DevicePath);
                                }
                                catch (Exception ex) { }
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }
        }
    }
}
