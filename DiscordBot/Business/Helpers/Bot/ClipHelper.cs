using DiscordBot.Data;
using DiscordBot.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DiscordBot.Business.Helpers.Bot;

public static class ClipHelper
{
    internal static async Task<AudioClip?> GetValidateCallCodeAsync(string callCode)
    {
        try
        {
            await using var context = new DatabaseContext();
            var audioClip = await context.AudioClips.AsNoTracking().FirstOrDefaultAsync(f => f.CallCode.Equals(callCode));
            if (audioClip == null)
                return null;

            if (File.Exists(audioClip.FilePath))
            {
                Log.Verbose("CallCode {callCode} with valid file exist.", callCode);
                return audioClip;
            }

            context.AudioClips.Remove(audioClip);
            await context.SaveChangesAsync();

            Log.Information("CallCode '{callCode}' existed, but has no valid file. It has been freed.", callCode);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not get clip for callCode '{callCode}'.", callCode);
            return null;
        }
    }

    /// <summary>
    /// Checks if the call code and the underlying file is valid. If not the call code is removed and free for another file to register.
    /// </summary>
    /// <param name="callCode">A value indicating if a <paramref name="callCode"/> is in use.</param>
    /// <returns><see langword="true"/> if the <see cref="AudioClip"/> occupied</returns>
    internal static async Task<bool> DoesCallCodeExistAsync(string callCode)
    {
        try
        {
            await using var context = new DatabaseContext();
            var existingCallCode = await context.AudioClips.FirstOrDefaultAsync(a => a.CallCode.Equals(callCode));
            if (existingCallCode == null)
                return false;

            if (File.Exists(existingCallCode.FilePath))
            {
                Log.Verbose("CallCode {callCode} with valid file exist.", callCode);
                return true;
            }

            context.AudioClips.Remove(existingCallCode);
            await context.SaveChangesAsync();

            Log.Information("CallCode '{callCode}' existed, but has no valid file. It has been freed.", callCode);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error validating callCode.");
            return true;
        }
    }

    internal static async Task<bool> AddNewClipAsync(AudioClip audioClip)
    {
        try
        {
            await using var context = new DatabaseContext();
            context.AudioClips.Add(audioClip);
            return await context.SaveChangesAsync() != 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "Could not add new clip.");
            return false;
        }
    }
}
