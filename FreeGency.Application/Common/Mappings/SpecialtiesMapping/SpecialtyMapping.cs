using System;
using System.Collections.Generic;
using System.Text;
using FreeGency.Application.Features.specialties.Dtos;
using FreeGency.Domain.Entities;

namespace FreeGency.Application.Common.Mappings.SpecialtiesMapping
{
    public static class SpecialtyMapping
    {
        public static SpecialtyDto ToDto(this Specialty specialty)
        {
            return new SpecialtyDto
            {
                Id = specialty.Id,
                NameEn = specialty.NameEn,
                NameAr = specialty.NameAr
            };
        }

        public static Specialty ToEntity(this CreateSpecialtyDto dto)
        {
            return new Specialty
            {
                Id = Guid.NewGuid(),
                NameEn = dto.NameEn,
                NameAr = dto.NameAr
            };
        }
    }
}
