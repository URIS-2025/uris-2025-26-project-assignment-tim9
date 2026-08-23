using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimelogService.Models.DTO.WorkPackage
{
    public class FlexibleTaskStatusConverter : JsonConverter<string>
    {
        private static readonly string[] KnownNames = { "ToDo", "InProgress", "InReview", "Done", "Blocked" };

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var index) &&
                index >= 0 && index < KnownNames.Length)
            {
                return KnownNames[index];
            }

            return string.Empty;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }
}
