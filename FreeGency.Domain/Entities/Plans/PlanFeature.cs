using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FreeGency.Domain.Entities.Plans
{
    public class PlanFeature
    {
        public Guid Id { get; set; }

        public Guid PlanId { get; set; }
        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;

        public FeatureType Feature { get; set; }

        public int? Limit { get; set; }
        public bool IsEnabled { get; set; }
    }
    public enum FeatureType
    {
        CreateProject,
        SendProposal,
        TeamMembers,
        ActiveProjects,
        FileStorage
    }
}
