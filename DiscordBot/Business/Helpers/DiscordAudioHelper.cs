using Discord.Audio;
using System.Diagnostics;

namespace DiscordBot.Business.Helpers;

internal sealed class DiscordAudioHelper(IAudioClient audioClient) : IDisposable
{
    private readonly AudioOutStream _audioOutStream = audioClient.CreatePCMStream(AudioApplication.Mixed);

    internal async Task PlayAudioAsync(string ressource)
    {
        using var ffmpegProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{ressource}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        }) ?? throw new Exception("Could not initialize ffmpeg process.");

        await using var ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;
        await ffmpegStream.CopyToAsync(_audioOutStream);
    }

    internal async Task FlushAsync()
    {
        await _audioOutStream.FlushAsync();
    }

    public void Dispose()
    {
        _audioOutStream.Dispose();
    }
}
