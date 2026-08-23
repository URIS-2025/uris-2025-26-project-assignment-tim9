using System.Text.Json.Serialization;

namespace TimelogService.Models.DTO.WorkPackage
{
    public class TaskDTO
    {
        public string Title { get; set; } = string.Empty;

        [JsonConverter(typeof(FlexibleTaskStatusConverter))]
        public string Status { get; set; } = string.Empty;
    }
}
