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
        var databaseDirectory = FileManager.GetDatabaseDirectory();
        var relativePath = Path.Combine(databaseDirectory, nameof(DiscordBot));
        optionsBuilder.UseSqlite($"Data source={relativePath}.db");
    }

    internal static bool CreateDefault()
    {
        try
        {
            var databaseDirectory = FileManager.GetDatabaseDirectory();
            if (!Directory.Exists(databaseDirectory))
                Directory.CreateDirectory(databaseDirectory);

            using var context = new DatabaseContext();
            if (!context.Database.EnsureCreated())
            {
                Log.Verbose("Database did not need to be created.");
                return true;
            }

            context.DiscordUsers.Add(new DiscordUser
            {
                Id = 229720939078615040,
                GlobalName = "flander_lander",
            });

            context.SaveChanges();
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
