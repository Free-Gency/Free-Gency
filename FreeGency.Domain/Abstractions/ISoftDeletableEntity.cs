
namespace FreeGency.Domain.Abstractions;

public interface ISoftDeletableEntity:IAuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}