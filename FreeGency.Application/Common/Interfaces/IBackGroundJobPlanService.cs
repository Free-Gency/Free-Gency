using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IBackGroundJobPlanService
    {
        Task ProcessPlansAsync();
    }
}
