using Api.Middleware;

namespace Api.Factory;

internal static class ApplicationFactory
{
    internal static WebApplication CustomBuild(this WebApplicationBuilder builder)
    {
        WebApplication app = builder.Build();

        app.UseMiddleware<CustomExceptionHandler>();

        app.UseCors(policy =>
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());

        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.AddSwagger();

        return app;
    }

    private static void AddSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            string environmentName = app.Environment.EnvironmentName[..3];

            options.SwaggerEndpoint(
                $"/swagger/v1.0/swagger.json",
                $"Swagger v1.0");
        });
    }
}