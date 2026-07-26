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
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
