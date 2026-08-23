namespace TimelogService.ServiceCalls.Project
{
    public enum ProjectMembershipStatus
    {
        Member,
        NotMember,
        ServiceUnavailable
    }

    public record ProjectMembershipResult(ProjectMembershipStatus Status);
}
