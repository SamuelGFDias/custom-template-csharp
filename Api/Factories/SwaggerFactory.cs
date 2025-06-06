using Application.Contracts;
using Microsoft.OpenApi.Models;

namespace Api.Factories;

public class SwaggerFactory : IFactory
{
    public void AddFactory(IHostApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1.0",
                new OpenApiInfo { Title = $"Swagger - {builder.Environment.EnvironmentName} 1.0", Version = "v1.0" });


            /* DESCOMENTE SE FOR USAR AUTENTICAÇÃO JWT
            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Por favor, insira 'Bearer' [espaço] e depois seu token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        []
                    }
                });
            
            */
        });
    }
}