using System.Text.Json.Serialization;

namespace StorySpoilerAPI.Models
{
    internal class APIResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg {  get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }
    }
}
