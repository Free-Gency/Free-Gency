
namespace FreeGency.Application.Common.Mappings.TaskMappings;

public class TaskMapping : Profile
{
    public TaskMapping()
    {
        CreateMap<ProjectTask, TaskDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AssigneeName,
                opt => opt.MapFrom(src => src.Assignee != null
                    ? $"{src.Assignee.FristName} {src.Assignee.LastName}".Trim()
                    : null))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Milestone.ProjectId))
            .ForMember(dest => dest.MilestoneTitle, opt => opt.MapFrom(src => src.Milestone.Title))
            .ForMember(dest => dest.ProjectTitle,
                opt => opt.MapFrom(src => src.Milestone.Project != null ? src.Milestone.Project.Title : string.Empty))
            .ForMember(dest => dest.CanManage, opt => opt.Ignore())
            .ForMember(dest => dest.IncompleteSubtasksCount,
                    opt => opt.MapFrom(src => src.Subtasks.Count(src => src.Status != Domain.Enums.TaskStatus.Done)))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dest => dest.ChecklistItems, opt => opt.MapFrom(src => src.ChecklistItems))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(dest => dest.TimeLogs, opt => opt.MapFrom(src => src.TimeLogs))
            .ForMember(dest => dest.Subtasks, opt => opt.MapFrom(src => src.Subtasks));

    
        CreateMap<TaskComment, TaskCommentDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => $"{src.User.FristName} {src.User.LastName}".Trim()));


        CreateMap<TaskChecklistItem, TaskChecklistItemDto>();


        CreateMap<TaskAttachment, TaskAttachmentDto>();


        CreateMap<TaskTimeLog, TaskTimeLogDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => $"{src.User.FristName} {src.User.LastName}".Trim()));


        CreateMap<TaskSubtask, TaskSubtaskDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
