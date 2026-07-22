using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class UserSpecialty : ISoftDeletableEntity, IBaseEntity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid UserId { get; set; }

        public Guid SpecialtyId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Specialty Specialty { get; set; } = null!;
    } 
}
