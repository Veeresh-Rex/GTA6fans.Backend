using System.Text.Json.Serialization;

namespace GTA6fans.Domain.Models;


public class RecaptchaVerificationRequest
{
    public string Secret { get; set; }
    public string Response { get; set; }
    public string RemoteIp { get; set; }
}

public class RecaptchaVerificationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("challenge_ts")]
    public DateTime ChallengeTimestamp { get; set; }

    [JsonPropertyName("hostname")]
    public string Hostname { get; set; }

    [JsonPropertyName("error-codes")]
    public string[] ErrorCodes { get; set; }
}