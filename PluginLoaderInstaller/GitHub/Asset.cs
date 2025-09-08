using Newtonsoft.Json;

namespace PulsarInstaller.GitHub;

public class Asset
{
    [JsonProperty("url")]
    public required Uri Url { get; set; }

    [JsonProperty("id")]
    public required long Id { get; set; }

    [JsonProperty("node_id")]
    public required string NodeId { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("label")]
    public required object Label { get; set; }

    [JsonProperty("uploader")]
    public required Author Uploader { get; set; }

    [JsonProperty("content_type")]
    public required string ContentType { get; set; }

    [JsonProperty("state")]
    public required string State { get; set; }

    [JsonProperty("size")]
    public required long Size { get; set; }

    [JsonProperty("digest")]
    public required string Digest { get; set; }

    [JsonProperty("download_count")]
    public required long DownloadCount { get; set; }

    [JsonProperty("created_at")]
    public required DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public required DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("browser_download_url")]
    public required Uri BrowserDownloadUrl { get; set; }
}
