using System;
using Application.Contracts;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Api.Factories;

public class SerialogFactory : IFactory
{
    public void AddFactory(IHostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.HostFiltering", LogEventLevel.Warning)
                    .Filter.ByExcluding(e => e.Exception != null)
                    .WriteTo.Console(
                         theme: AnsiConsoleTheme.Literate,
                         restrictedToMinimumLevel: LogEventLevel.Information,
                         outputTemplate:
                         "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Method} {Path} ({StatusCode}) {Message:lj}{NewLine}"
                     )
                    .CreateLogger();
    }
}
