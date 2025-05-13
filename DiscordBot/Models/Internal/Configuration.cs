using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Models.Internal;

public class Configuration
{
    public Discord Discord { get; set; }

    [Required]
    public string DanbooruToken { get; set; }
}