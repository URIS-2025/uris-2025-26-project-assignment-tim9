namespace WorkPackageService.Models.DTO.WorkPackage
{
    public class WorkPackageUpdateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Budget { get; set; }
    }
}