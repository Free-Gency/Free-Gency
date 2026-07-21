using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class ClientProfile : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid UserId { get; set; }
    public string? ProfileImage { get; set; }
    public string? Bio { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;

    public virtual User User { get; set; } = null!;
}
