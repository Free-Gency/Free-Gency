using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Abstractions
{
    public interface IBaseEntity
    {
        public Guid Id { get; set; }
    }
}
