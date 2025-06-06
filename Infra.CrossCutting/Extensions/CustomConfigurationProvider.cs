using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Infra.CrossCutting.Extensions;

public static class CustomConfigurationProvider
{
    private const string CONFIGURATION_FOLDER_NAME = "ConfigSettings";

    public static IConfiguration Build(IHostEnvironment env)
    {
        return Build(env.EnvironmentName);
    }

    public static IConfiguration Build(string env)
    {
        var config = new ConfigurationBuilder();

        config.AddJsonFiles(env);

        if (!env.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            string envPath = Path.Combine(AppContext.BaseDirectory, CONFIGURATION_FOLDER_NAME, Env.DEFAULT_ENVFILENAME);

            if (!File.Exists(envPath))
                Console.WriteLine($"[CONFIG] Falha ao carregar variáveis de ambiente de: {envPath}");

            Env.Load(envPath);
        }
        else
        {
            Env.Load();
        }

        config.ReplaceEnvVariables();

        return config.Build();
    }

    public static void ReplaceEnvVariables(this ConfigurationBuilder config)
    {
        IConfigurationRoot tempBuilder = config.Build();

        var updatedValues = new Dictionary<string, string>();

        foreach (KeyValuePair<string, string?> kvp in tempBuilder.AsEnumerable())
            if (kvp.Value is { } value
             && !string.IsNullOrEmpty(value)
             && value.StartsWith("__")
             && value.EndsWith("__"))
            {
                string envVarName = value.Trim('_');
                string? envVarValue = Env.GetString(envVarName);

                if (!string.IsNullOrEmpty(envVarValue))
                    updatedValues[kvp.Key] = envVarValue;
                else
                    updatedValues[kvp.Key] = value;
            }
            else
            {
                updatedValues[kvp.Key] = kvp.Value ?? string.Empty;
            }

        if (updatedValues.Count > 0) config.AddInMemoryCollection(updatedValues!);
    }

    public static string AddJsonFiles(this ConfigurationBuilder config, string env)
    {
        string settingsFile = $"appsettings{(env == "Production" ? "" : $".{env}")}.json";

        string path = Path.Combine(CONFIGURATION_FOLDER_NAME, settingsFile);

        config.AddJsonFile(path, true, true);
        return path;
    }
}