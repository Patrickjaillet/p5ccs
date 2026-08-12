using CommunityToolkit.Mvvm.ComponentModel;
using P5CCS.App.Services;
using P5CCS.Core.Versioning;

namespace P5CCS.App.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public const string ProductName = "Processing 5 - Creative Coding Station";
    public const string CopyrightText = "Copyright © 2026 Patrick JAILLET — All rights reserved";
    public const string ContactEmail = "sandefjord.development@proton.me";
    public const string WebsiteUrl = "https://patrickjaillet.github.io/p5ccs";

    public static readonly Uri ContactMailtoUri = new($"mailto:{ContactEmail}");
    public static readonly Uri WebsiteUri = new(WebsiteUrl);

    public AboutViewModel(IVersionService versionService)
    {
        VersionText = $"Version {versionService.InformationalVersion}";
        LicenseText = EmbeddedTextResource.Read("P5CCS.App.Resources.License.txt");
        ThirdPartyNoticesText = EmbeddedTextResource.Read("P5CCS.App.Resources.ThirdPartyNotices.txt");
    }

    [ObservableProperty]
    private string _versionText;

    [ObservableProperty]
    private string _licenseText;

    [ObservableProperty]
    private string _thirdPartyNoticesText;
}
