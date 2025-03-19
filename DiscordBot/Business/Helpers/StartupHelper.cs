using Microsoft.Extensions.Configuration;
using Serilog;

namespace DiscordBot.Business.Helpers;

internal static class StartupHelper
{
    internal static string? LogGetVariable(string? variable)
    {
        if (variable == null)
        {
            Log.Warning("Tried to retriver a null enviroment variable.");
            return null;
        }

        var value = Environment.GetEnvironmentVariable(variable);
        if (value == null)
            Log.Verbose("{variable}: '{value}'", variable, value);
        else
            Log.Warning($"Tried to get variable '{variable}' but failed.");

        return value;
    }

    internal static string? GetLogValue(this IConfiguration configuration, string variableName)
    {
        var value = configuration[variableName];
        if (string.IsNullOrWhiteSpace(value))
            Log.Warning("Tried to get the environment variable '{name}' but failed.", variableName);
        else
            Log.Verbose("{variable}: '{value}'", variableName, value);
        return value;
    }
}
