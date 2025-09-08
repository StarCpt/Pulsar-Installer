using Newtonsoft.Json;

namespace PulsarInstaller.GitHub;

public class Release
{
    [JsonProperty("url")]
    public required Uri Url { get; set; }

    [JsonProperty("assets_url")]
    public required Uri AssetsUrl { get; set; }

    [JsonProperty("upload_url")]
    public required string UploadUrl { get; set; }

    [JsonProperty("html_url")]
    public required Uri HtmlUrl { get; set; }

    [JsonProperty("id")]
    public required long Id { get; set; }

    [JsonProperty("author")]
    public required Author Author { get; set; }

    [JsonProperty("node_id")]
    public required string NodeId { get; set; }

    [JsonProperty("tag_name")]
    public required string TagName { get; set; }

    [JsonProperty("target_commitish")]
    public required string TargetCommitish { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("draft")]
    public required bool Draft { get; set; }

    [JsonProperty("immutable")]
    public required bool Immutable { get; set; }

    [JsonProperty("prerelease")]
    public required bool Prerelease { get; set; }

    [JsonProperty("created_at")]
    public required DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public required DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("published_at")]
    public required DateTimeOffset PublishedAt { get; set; }

    [JsonProperty("assets")]
    public required Asset[] Assets { get; set; }

    [JsonProperty("tarball_url")]
    public required Uri TarballUrl { get; set; }

    [JsonProperty("zipball_url")]
    public required Uri ZipballUrl { get; set; }

    [JsonProperty("body")]
    public required string Body { get; set; }
}
