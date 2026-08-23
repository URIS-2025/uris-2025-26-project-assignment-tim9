using AutoMapper;
using WorkPackageService.Models.DTO.TaskDTOs;
using Task = WorkPackageService.Models.Task;

namespace WorkPackageService.Profiles
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<Task, TaskDisplayDTO>();

            CreateMap<TaskCreateDTO, Task>();

            CreateMap<TaskUpdateDTO, Task>()
                .ForMember(dest => dest.Title, opt => opt.Condition(src => src.Title != null))
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status != null))
                .ForMember(dest => dest.Priority, opt => opt.Condition(src => src.Priority != null))
                .ForMember(dest => dest.AssigneeId, opt => opt.Condition(src => src.AssigneeId != null))
                .ForMember(dest => dest.ApproverId, opt => opt.Condition(src => src.ApproverId != null))
                .ForMember(dest => dest.DueDate, opt => opt.Condition(src => src.DueDate != null));
        }
    }
}
