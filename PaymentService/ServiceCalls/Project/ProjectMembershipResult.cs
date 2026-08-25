namespace PaymentService.ServiceCalls.Project
{
    public enum ProjectMembershipStatus
    {
        Member,
        NotMember,
        ServiceUnavailable
    }

    public record ProjectMembershipResult(ProjectMembershipStatus Status);
}
