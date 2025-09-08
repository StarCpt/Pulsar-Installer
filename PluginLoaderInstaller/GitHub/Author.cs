using Newtonsoft.Json;

namespace PulsarInstaller.GitHub;

public class Author
{
    [JsonProperty("login")]
    public required string Login { get; set; }

    [JsonProperty("id")]
    public required long Id { get; set; }

    [JsonProperty("node_id")]
    public required string NodeId { get; set; }

    [JsonProperty("avatar_url")]
    public required Uri AvatarUrl { get; set; }

    [JsonProperty("gravatar_id")]
    public required string GravatarId { get; set; }

    [JsonProperty("url")]
    public required Uri Url { get; set; }

    [JsonProperty("html_url")]
    public required Uri HtmlUrl { get; set; }

    [JsonProperty("followers_url")]
    public required Uri FollowersUrl { get; set; }

    [JsonProperty("following_url")]
    public required string FollowingUrl { get; set; }

    [JsonProperty("gists_url")]
    public required string GistsUrl { get; set; }

    [JsonProperty("starred_url")]
    public required string StarredUrl { get; set; }

    [JsonProperty("subscriptions_url")]
    public required Uri SubscriptionsUrl { get; set; }

    [JsonProperty("organizations_url")]
    public required Uri OrganizationsUrl { get; set; }

    [JsonProperty("repos_url")]
    public required Uri ReposUrl { get; set; }

    [JsonProperty("events_url")]
    public required string EventsUrl { get; set; }

    [JsonProperty("received_events_url")]
    public required Uri ReceivedEventsUrl { get; set; }

    [JsonProperty("type")]
    public required string Type { get; set; }

    [JsonProperty("user_view_type")]
    public required string UserViewType { get; set; }

    [JsonProperty("site_admin")]
    public required bool SiteAdmin { get; set; }
}
