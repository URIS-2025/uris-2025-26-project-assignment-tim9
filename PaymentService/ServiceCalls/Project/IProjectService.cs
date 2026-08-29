using PaymentService.Models.DTO.Project;

namespace PaymentService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<ProjectInfoDTO?> GetProjectInfoAsync(Guid projectId);
        Task<ProjectMembershipResult> CheckMembershipAsync(Guid projectId, Guid userId);

        //projekti na kojima je korisnik clan; null znaci da servis nije odgovorio
        Task<IReadOnlyCollection<Guid>?> GetProjectIdsForUserAsync(Guid userId);
    }
}
