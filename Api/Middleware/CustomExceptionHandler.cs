using System.Net;
using System.Text.Json;
using Serilog;
using Serilog.Context;

namespace Api.Middleware;

public class CustomExceptionHandler(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("Method", context.Request.Method))
        using (LogContext.PushProperty("Path", context.Request.Path))
        {
            try
            {
                await next(context);

                if (context.Response.StatusCode == StatusCodes.Status200OK)
                    using (LogContext.PushProperty("StatusCode", StatusCodes.Status200OK))
                    {
                        Log.Information("");
                    }
            } catch (Exception ex)
            {
                string errorMessage = ex.Message;

                (int statusCode, string message) = ex switch
                {
                    ArgumentNullException     => (StatusCodes.Status400BadRequest, errorMessage),
                    InvalidOperationException => (StatusCodes.Status400BadRequest, errorMessage),
                    ArgumentException         => (StatusCodes.Status400BadRequest, errorMessage),
                    not null when ex.InnerException != null => (StatusCodes.Status400BadRequest,
                                                                ex.InnerException.Message),
                    _ => ((int)HttpStatusCode.InternalServerError, errorMessage)
                };
                using (LogContext.PushProperty("StatusCode", statusCode))
                {
                    await HandleErrorAsync(message, statusCode, context);
                }
            }
        }
    }


    private async Task HandleErrorAsync(string message, int statusCode, HttpContext context)
    {
        Log.Error("Erro ao processar: {Mensagem}", message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = message }));
    }
}