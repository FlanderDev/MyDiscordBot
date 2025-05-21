using Serilog;

namespace DiscordBot.Business.Helpers.Bot;

internal static class StartupHelper
{
    internal static string? LogGetVariable(string? variable)
    {
        if (variable == null)
        {
            Log.Warning("Tried to retriever a null environment variable.");
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

    internal static bool RecursivelyNotNullOrWhiteSpace(object instance)
    {
        foreach (var property in instance.GetType().GetProperties())
        {
            var propertyValue = property.GetValue(instance);
            switch (propertyValue)
            {
                case null:
                    return false;
                case string text when !string.IsNullOrWhiteSpace(text):
                    continue;
            }

            // Probably should check for more but classes, but meh
            if (property.PropertyType.IsClass &&
                !property.PropertyType.Name.Equals("String", StringComparison.OrdinalIgnoreCase) &&
                !RecursivelyNotNullOrWhiteSpace(propertyValue))
                return false;
        }
        return true;
    }

    internal static bool MoveRequiredFiles()
    {
        const string opusDll = "opus.dll";
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                Log.Information("Not running on Windows, skipping fix for (lib)Opus.dll.");
                return true;
            }

            var shouldFilePath = Path.Combine(FileHelper.BaseDirectory, opusDll);
            if (File.Exists(shouldFilePath))
            {
                Log.Verbose("{fileName} already exists.", opusDll);
                return true;
            }

            // Yes really!
            var processorArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
            var opusFilePath= Directory
                .EnumerateFiles(FileHelper.BaseDirectory, "libopus.dll", SearchOption.AllDirectories)
                .FirstOrDefault(f => f.Contains($"win-{processorArchitecture}", StringComparison.OrdinalIgnoreCase));
            if (opusFilePath == null)
            {
                Log.Error("Could not find opus file.");
                return false;
            }

            Log.Verbose("Moving file from '{origin}' to '{target}'.", opusFilePath, shouldFilePath);

            File.Move(opusFilePath, shouldFilePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Trying to locate {fileName} caused an error.", opusDll);
            return false;
        }
    }
}
