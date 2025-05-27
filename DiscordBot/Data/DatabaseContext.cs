using DiscordBot.Business.Helpers.Bot;
using DiscordBot.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DiscordBot.Data;

public sealed class DatabaseContext : DbContext
{
    public DbSet<AudioClip> AudioClips => Set<AudioClip>();
    public DbSet<DiscordUser> DiscordUsers => Set<DiscordUser>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var databaseDirectory = FileHelper.GetDatabaseDirectory();
        if (!Directory.Exists(databaseDirectory))
            Directory.CreateDirectory(databaseDirectory);

        var relativePath = Path.Combine(databaseDirectory, nameof(DiscordBot));
        optionsBuilder.UseSqlite($"Data source={relativePath}.db");
    }

    internal static async Task<bool> CreateDefaultAsync()
    {
        try
        {
            var databaseDirectory = FileHelper.GetDatabaseDirectory();
            if (!Directory.Exists(databaseDirectory))
                Directory.CreateDirectory(databaseDirectory);

            await using var context = new DatabaseContext();
            if (!await context.Database.EnsureCreatedAsync())
            {
                Log.Verbose("Database did not need to be created.");
                return true;
            }

            var araUrl = $"https://faunaraara.com/sounds/ara-{77}.mp3";
            var result = await FileHelper.GetLocalResourceOrDownloadAsync($"ara-{77}.mp3", araUrl) ?? throw new Exception("Could not find or download file.");
            context.AudioClips.Add(new AudioClip
            {
                CallCode = "default",
                DiscordUserId = 229720939078615040,
                FilePath = result,
                DiscordUser = new DiscordUser
                {
                    Id = 229720939078615040,
                    GlobalName = "flander_lander",
                    Username = "Flan",
                    Administrator = true
                }
            });


            await context.SaveChangesAsync();
            Log.Information("Created default database.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not create default database.");
            return false;
        }
    }
}
