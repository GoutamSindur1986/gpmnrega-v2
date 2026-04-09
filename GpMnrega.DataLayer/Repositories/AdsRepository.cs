using GpMnrega.DataLayer.Models;
using System.Text.Json;

namespace GpMnrega.DataLayer.Repositories;

/// <summary>
/// Reads ads from wwwroot/data/ads.json.
/// Edit that file on the server to add/remove/reorder ads — no recompile needed.
/// Phase 3: swap this implementation for a Dapper DB query against the Ads table.
/// </summary>
public interface IAdsRepository
{
    Task<List<ProductAd>> GetLoginPageAdsAsync();
}

public class AdsRepository : IAdsRepository
{
    private readonly string _jsonPath;

    public AdsRepository(string webRootPath)
    {
        _jsonPath = Path.Combine(webRootPath, "data", "ads.json");
    }

    public async Task<List<ProductAd>> GetLoginPageAdsAsync()
    {
        try
        {
            if (!File.Exists(_jsonPath))
                return GetFallbackAds();

            var json = await File.ReadAllTextAsync(_jsonPath);
            var ads = JsonSerializer.Deserialize<List<ProductAd>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (ads ?? GetFallbackAds())
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder)
                .ToList();
        }
        catch
        {
            return GetFallbackAds();
        }
    }

    private static List<ProductAd> GetFallbackAds() => new()
    {
        new() { AdId=1, Title="WebChat Automation", ShortDesc="Automate WhatsApp, Web & SMS conversations with AI.", CtaText="Notify Me", CtaUrl="#notify", IconClass="bi-chat-dots-fill", BadgeText="Coming Soon", BadgeColor="warning", IsActive=true, SortOrder=1 },
        new() { AdId=2, Title="Instagram Automation", ShortDesc="Schedule posts, auto-reply DMs, grow your IG presence.", CtaText="Join Waitlist", CtaUrl="#notify", IconClass="bi-instagram", BadgeText="Coming Soon", BadgeColor="warning", IsActive=true, SortOrder=2 },
        new() { AdId=3, Title="Facebook Automation", ShortDesc="Manage pages, auto-respond to comments at scale.", CtaText="Join Waitlist", CtaUrl="#notify", IconClass="bi-facebook", BadgeText="Coming Soon", BadgeColor="warning", IsActive=true, SortOrder=3 },
        new() { AdId=4, Title="Advertise Here", ShortDesc="Reach 1500+ GP officials daily. Contact us.", CtaText="Contact Us", CtaUrl="/contact", IconClass="bi-megaphone-fill", BadgeText="Sponsor", BadgeColor="info", IsActive=true, SortOrder=4 }
    };
}
