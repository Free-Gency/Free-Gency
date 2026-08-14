namespace FreeGency.Application.Features.UserProfiles.DTOs
{
    public record DeveloperProfileDto
    {
        public string FullName { get; set; } = default!;
        public string? ProfileImage { get; set; }
        public string? Bio { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }

        public List<string> UserInterests { get; set; } = [];
        public List<string> UserSpecialties { get; set; } = [];
        public List<string> UserSkills { get; set; } = [];
    }
}
