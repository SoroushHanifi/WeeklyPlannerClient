using System.Net;
using System.Text.Json;

namespace WeeklyPlannerAPI.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            success = false,
            message = "خطای داخلی سرور. لطفاً دوباره تلاش کنید.",
            detail  = ex.Message // در production این را حذف کنید
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
