﻿using System.Text.Json.Serialization;

namespace Story_spoiler.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyid")]
        public string? StoryId { get; set; }
    }
}
