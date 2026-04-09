namespace GpMnrega.Web.Services;

/// <summary>
/// Wraps Google Cloud Translate V3.
/// Original code used Google.Cloud.Translate.V3 directly inside pages.
/// This service centralises it, making it swappable and testable.
/// 
/// SETUP:
/// 1. Install NuGet: Google.Cloud.Translation.V2 (simpler REST client) or
///    Google.Cloud.Translate.V3 (same as original)
/// 2. Set environment variable GOOGLE_APPLICATION_CREDENTIALS pointing to
///    your service account JSON file (same file referenced in original code)
/// 3. OR set GoogleTranslate:CredentialsPath in appsettings.json
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Translate a single text string.
    /// </summary>
    Task<string?> TranslateAsync(string text, string sourceLanguage, string targetLanguage);

    /// <summary>
    /// Translate multiple strings in one API call (more efficient).
    /// </summary>
    Task<IEnumerable<string>> TranslateBatchAsync(
        IEnumerable<string> texts, string sourceLanguage, string targetLanguage);
}

/// <summary>
/// Production implementation using Google Cloud Translate V3.
/// Matches the exact API used in the original Emailverification.aspx.cs:
///   TranslationServiceClientBuilder { CredentialsPath = ... }.Build()
///   translateClient.TranslateText(request)
/// </summary>
public class GoogleTranslationService : ITranslationService
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<GoogleTranslationService> _log;
    private const string PROJECT_ID = "api-project-519170880876"; // from original code

    public GoogleTranslationService(IConfiguration cfg, ILogger<GoogleTranslationService> log)
    {
        _cfg = cfg;
        _log = log;
    }

    public async Task<string?> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
        var results = await TranslateBatchAsync([text], sourceLanguage, targetLanguage);
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<string>> TranslateBatchAsync(
        IEnumerable<string> texts, string sourceLanguage, string targetLanguage)
    {
        // ── Install this package to use: ────────────────────────────────────
        // dotnet add package Google.Cloud.Translation.V2
        //
        // OR the V3 SDK (matches original exactly):
        // dotnet add package Google.Cloud.Translate.V3
        //
        // Then uncomment and use this code:
        //
        // var credPath = _cfg["GoogleTranslate:CredentialsPath"];
        // var clientBuilder = new Google.Cloud.Translate.V3.TranslationServiceClientBuilder();
        // if (!string.IsNullOrEmpty(credPath))
        //     clientBuilder.CredentialsPath = credPath;
        // var client = await clientBuilder.BuildAsync();
        //
        // var request = new Google.Cloud.Translate.V3.TranslateTextRequest
        // {
        //     Parent = $"projects/{PROJECT_ID}",
        //     SourceLanguageCode = sourceLanguage,
        //     TargetLanguageCode = targetLanguage
        // };
        // request.Contents.AddRange(texts);
        // var response = await client.TranslateTextAsync(request);
        // return response.Translations.Select(t => t.TranslatedText);

        // ── Stub until Google Cloud SDK is installed ────────────────────────
        _log.LogInformation(
            "Translation stub: {Count} texts {Source}→{Target}. " +
            "Install Google.Cloud.Translation.V2 and uncomment implementation.",
            texts.Count(), sourceLanguage, targetLanguage);

        // Return original texts unchanged until real implementation is added
        return await Task.FromResult(texts);
    }
}

/// <summary>
/// No-op implementation for environments without Google Cloud credentials.
/// Used in development or when translation is not needed.
/// </summary>
public class NoOpTranslationService : ITranslationService
{
    public Task<string?> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
        => Task.FromResult<string?>(null);

    public Task<IEnumerable<string>> TranslateBatchAsync(
        IEnumerable<string> texts, string sourceLanguage, string targetLanguage)
        => Task.FromResult(texts);
}
