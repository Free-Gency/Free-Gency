using System.ComponentModel.DataAnnotations;

namespace FreeGency.Infrastructure.Integrations.Cloudinary;

public class CloudinaryOptions
{
    public static string NameSection = "Cloudinary";

    [Required]
    public string CloudName { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string ApiSecret { get; set; } = string.Empty;
}
