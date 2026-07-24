using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class UserJoinRequestSpecificationParam
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value <= 0 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value switch
            {
                <= 0 => 10,
                > 20 => 20,
                _ => value
            };
        }

        public TeamJoinRequestStatus? Status { get; set; }
    }
}
