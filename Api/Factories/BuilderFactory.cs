using Api.Factories;
using Application.Contracts;
using Infra.CrossCutting;
using Infra.CrossCutting.Extensions;
using Infra.IoC;
using Serilog;

namespace Api.Factories;

internal static class BuilderFactory
{
    internal static WebApplicationBuilder GenerateWebApplicationBuilder(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.AddConfiguration();

        foreach (IFactory instance in
                 from type in AppDomain.CurrentDomain.GetAssemblies()
                                       .SelectMany(a => a.GetTypes())
                                       .Where(t => t is
                                       {
                                           IsClass: true, IsAbstract: false, IsInterface: false,
                                           IsGenericType: false
                                       }
                                                && typeof(IFactory).IsAssignableFrom(t))
                 let instance = (IFactory)Activator.CreateInstance(type)!
                 select instance)
        {
            instance.AddFactory(builder);
        }

        builder.Host.UseSerilog();

        return builder;
    }

    private static void AddConfiguration(this WebApplicationBuilder builder)
    {
        IConfiguration configuration = CustomConfigurationProvider.Build(builder.Environment);

        builder.Configuration.AddConfiguration(configuration);

        builder.AddProviders<IProvider>();
    }
}