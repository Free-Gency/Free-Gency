using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ProjectFiles
    {
        public Guid ProjectId { get; set; }
        public Guid MilestoneId {  get; set; }
        public Guid UploadedByUserId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public FileKind FileKind { get; set; }
        
    }
}
