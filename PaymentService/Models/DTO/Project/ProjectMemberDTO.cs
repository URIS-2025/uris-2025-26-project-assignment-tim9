namespace PaymentService.Models.DTO.Project
{
    //podskup ProjectMemberDto iz Project servisa - treba nam samo ko je clan i da li je aktivan
    public class ProjectMemberDTO
    {
        public Guid UserId { get; set; }
        public bool Status { get; set; }
    }
}
