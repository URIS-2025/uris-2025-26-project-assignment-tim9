using System.Text.Json;
using TimelogService.Models.DTO.WorkPackage;

namespace TimelogService.Tests
{
    public class FlexibleTaskStatusConverterTests
    {
        [Theory]
        [InlineData(0, "ToDo")]
        [InlineData(1, "InProgress")]
        [InlineData(2, "InReview")]
        [InlineData(3, "Done")]
        [InlineData(4, "Blocked")]
        public void Deserializes_NumericStatus_ToItsName(int numericValue, string expectedName)
        {
            var json = $"{{\"title\":\"Some Task\",\"status\":{numericValue}}}";

            var task = JsonSerializer.Deserialize<TaskDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(expectedName, task!.Status);
        }

        [Fact]
        public void Deserializes_StringStatus_AsIs()
        {
            var json = "{\"title\":\"Some Task\",\"status\":\"InProgress\"}";

            var task = JsonSerializer.Deserialize<TaskDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal("InProgress", task!.Status);
        }

        [Fact]
        public void Deserializes_OutOfRangeNumericStatus_ToEmptyRatherThanThrowing()
        {
            var json = "{\"title\":\"Some Task\",\"status\":99}";

            var task = JsonSerializer.Deserialize<TaskDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(string.Empty, task!.Status);
        }
    }
}
