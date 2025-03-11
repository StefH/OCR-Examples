using System.Text.Json.Serialization;

namespace OlmOcrExample.Models;

public class OlmResponse
{
    [JsonPropertyName("primary_language")]
    public string PrimaryLanguage { get; set; } = "en";

    [JsonPropertyName("is_rotation_valid")]
    public bool IsRotationValid { get; set; }

    [JsonPropertyName("rotation_correction")]
    public int RotationCorrection { get; set; }

    [JsonPropertyName("is_table")]
    public bool IsTable { get; set; }

    [JsonPropertyName("is_diagram")]
    public bool IsDiagram { get; set; }

    [JsonPropertyName("natural_text")]
    public string NaturalText { get; set; } = string.Empty;
}