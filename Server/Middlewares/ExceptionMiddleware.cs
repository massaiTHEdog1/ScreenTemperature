

namespace ScreenTemperature.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment webHostEnvironment)
{

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            context.Response.StatusCode = 500;

            if(webHostEnvironment.IsDevelopment())
            {
                await context.Response.WriteAsJsonAsync(new { message = ex.Message });
            }
        }
    }
}