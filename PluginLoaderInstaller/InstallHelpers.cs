using Newtonsoft.Json;
using PulsarInstaller.GitHub;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace PulsarInstaller;

public static class InstallHelpers
{
    const string LATEST_RELEASE_URL = "https://api.github.com/repos/SpaceGT/Pulsar/releases/latest";

    public static async Task<(SemanticVersion? version, Release latestReleaseInfo)> TryGetLatestVersion()
    {
        try
        {
            // check latest github release version

            using var webClient = new HttpClient();
            webClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // required for github API

            // get api response json
            using var response = await webClient.GetStreamAsync(LATEST_RELEASE_URL);

            // deserialize json
            using var responseReader = new StreamReader(response);
            using var responseJsonReader = new JsonTextReader(responseReader);
            Release latestReleaseInfo = JsonSerializer.CreateDefault().Deserialize<Release>(responseJsonReader)!;

            // parse version from release name
            // the name of the release (not release tag) should be in this format: Pulsar-v1.2.3
            return (SemanticVersion.Parse(latestReleaseInfo.Name.Replace("Pulsar-", "")), latestReleaseInfo);
        }
        catch
        {
            return default; // assume latest if can't get latest release version
        }
    }

    public static async Task<ZipArchive?> DownloadReleaseAsset(Release releaseInfo)
    {
        // find first valid zip asset
        string assetEndsWithStr = releaseInfo.Name + ".zip"; // Pulsar-v1.2.3.zip
        var assetToDownload = releaseInfo.Assets.FirstOrDefault(i => i.Name.EndsWith(assetEndsWithStr, StringComparison.OrdinalIgnoreCase));

        if (assetToDownload is null)
            return null;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // required for github API

        // download zip
        var response = await client.GetStreamAsync(assetToDownload.BrowserDownloadUrl);
        return new ZipArchive(response);
    }
}
