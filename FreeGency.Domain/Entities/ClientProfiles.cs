using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ClientProfiles
    {
           public string UserId { get; set; }
           public string ProfileImage { get; set; }
           public string Bio { get; set;  }
           public decimal AverageRating { get; set; }
           public int RatingCount { get; set; }

    }
}
