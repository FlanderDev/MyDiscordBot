using Discord;
using DiscordBot.Models.Enteties;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Database;

public class DatabaseContext : DbContext
{
    public DbSet<AudioClip> AudioClips => Set<AudioClip>();
    public DbSet<DiscordUser> DiscordUsers => Set<DiscordUser>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data source={nameof(DiscordBot)}.db");
    }
}
