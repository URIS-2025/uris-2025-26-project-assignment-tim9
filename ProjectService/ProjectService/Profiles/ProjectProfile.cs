using AutoMapper;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.DTO.ProjectMemberDtos;
using ProjectService.Models.DTO.MilestoneDtos;
using ProjectService.Models.DTO.RequirementsDtos;

namespace ProjectService.Profiles
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            // Project
            CreateMap<ProjectCreationDto, Project>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Project, ProjectConfirmationDto>();

            // ProjectMember
            CreateMap<ProjectMemberCreationDto, ProjectMember>().ReverseMap();
            CreateMap<ProjectMember, ProjectMemberDto>().ReverseMap();
            CreateMap<ProjectMember, ProjectMemberConfirmationDto>();

            // Milestone
            CreateMap<MilestoneCreationDto, Milestone>().ReverseMap();
            CreateMap<Milestone, MilestoneDto>().ReverseMap();
            CreateMap<Milestone, MilestoneConfirmationDto>();

            // Requirements
            CreateMap<RequirementsCreationDto, Requirements>().ReverseMap();
            CreateMap<Requirements, RequirementsDto>().ReverseMap();
            CreateMap<Requirements, RequirementsConfirmationDto>();
        }
    }
}
