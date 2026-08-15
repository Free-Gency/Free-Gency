
namespace FreeGency.Domain.Abstractions;

public interface IAuditableEntity:IBaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

}
