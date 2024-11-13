using DiscordBot.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Database;

public class DatabaseContext : DbContext
{
    public DbSet<AudioClip> AudioClips => Set<AudioClip>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data source={nameof(DiscordBot)}.db");
    }
}
