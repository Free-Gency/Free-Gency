using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.specialties.Dtos
{
    public sealed class UpdateSpecialtyDto
    {
        public Guid Id { get; set; }

        public string NameEn { get; set; } = string.Empty;

        public string NameAr { get; set; } = string.Empty;
    }
}
