namespace FreeGency.Application.Features.UserProfiles.DTOs
{
    public record ClientProfileDto
    {
        public string FullName { get; set; } = default!;
        public string? ProfileImage { get; set; }
        public string? Bio { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }

        public List<string> OwnedTeams { get; set; } = [];
        public bool IsVerified { get; set; }
        public string? Country { get; set; }
        public List<Project> PostedProjects { get; set; } = [];
        public List<SocialLink> SocialLinks { get; set; } = [];
        public List<Review> ReviewsReceived { get; set; } = [];
        public List<PortfolioProject> PortfolioProjects { get; set; } = [];


        public List<string> UserInterests { get; set; } = [];
        public List<string> UserSpecialties { get; set; } = [];
        public List<string> UserSkills { get; set; } = [];
    }
}
