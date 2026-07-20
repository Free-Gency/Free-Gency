using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreeGency.Domain.Entities;

public class Review : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
  
    public Guid ProjectId { get; set; }
    public Guid ReviewerUserId { get; set; }
    public RevieweeType RevieweeType { get; set; }
    public Guid? RevieweeTeamId { get; set; }
    public Guid? RevieweeUserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public virtual Project Project { get; set; } = null!;
    public virtual User ReviewerUser { get; set; } = null!;
    public virtual Team? RevieweeTeam { get; set; }
    public virtual User? RevieweeUser { get; set; }
}
