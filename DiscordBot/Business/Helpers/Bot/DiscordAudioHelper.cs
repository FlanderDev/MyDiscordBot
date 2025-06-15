using Discord.Audio;
using System.Diagnostics;

namespace DiscordBot.Business.Helpers.Bot;

internal static class DiscordAudioHelper
{
    internal static async Task PlayAudioAsync(this IAudioClient audioClient, string resource, CancellationToken cancellationToken = default)
    {
        using var audioOutStream = audioClient.CreateDirectPCMStream(AudioApplication.Mixed);
        using var ffmpegProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{resource}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        }) ?? throw new Exception("Could not initialize ffmpeg process.");

        await using var ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;
        await ffmpegStream.CopyToAsync(audioOutStream, cancellationToken);
        await audioOutStream.FlushAsync(cancellationToken);
    }
}
