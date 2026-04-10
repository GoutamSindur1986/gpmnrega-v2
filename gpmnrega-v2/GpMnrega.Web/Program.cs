using GpMnrega.DataLayer.Repositories;
using GpMnrega.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Razor Pages ──────────────────────────────────────────────
builder.Services.AddControllersWithViews();  // controllers + .cshtml views
builder.Services.AddRazorPages();            // Razor Pages (/Pages folder)

// ── Data layer ─────────────────────────────────────────────────────
var conn = builder.Configuration.GetConnectionString("dbgpmnregadev")!;
builder.Services.AddScoped<IGpUserRepository>(_ => new GpUserRepository(conn));
builder.Services.AddScoped<IDeptUserRepository>(_ => new DeptUserRepository(conn));
builder.Services.AddScoped<IGpCodeRepository>(_ => new GpCodeRepository(conn));
builder.Services.AddScoped<IAdsRepository>(sp =>
    new AdsRepository(sp.GetRequiredService<IWebHostEnvironment>().WebRootPath));

// ── App services ───────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICookieObfuscationService, CookieObfuscationService>();
// Use GoogleTranslationService in production; swap to NoOpTranslationService for dev
builder.Services.AddScoped<ITranslationService, GoogleTranslationService>();
builder.Services.AddHttpClient<INicProxyService, NicProxyService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("NicProxy:TimeoutSeconds", 120));
});

// ── Cookie authentication (replaces FormsAuthentication) ───────────
var authConfig = builder.Configuration.GetSection("Auth");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = authConfig["CookieName"] ?? ".S3KN_AUTH2";
        options.Cookie.HttpOnly = true;          // NOT readable by JS
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(
            authConfig.GetValue<int>("CookieExpireDays", 1));
        options.SlidingExpiration = true;
        // Redirect unauthenticated API calls to 401, not login page
        options.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// ── Data Protection — persist keys so auth cookies survive app restarts ─
// Without this, in-memory keys regenerate on each restart, invalidating
// all existing session cookies and causing 401 on every API call.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new System.IO.DirectoryInfo(
        builder.Configuration["Auth:DataProtectionKeysPath"]
        ?? System.IO.Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("GpMnrega");

// ── Session (for temp data between requests) ───────────────────────
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(60);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// ── CORS (for Blazor WASM calls to our own API) ────────────────────
builder.Services.AddCors(o => o.AddPolicy("WasmPolicy", p =>
    p.WithOrigins(builder.Configuration["AppSettings:SiteUrl"] ?? "https://gpmnrega.com")
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Blazor WASM files are published to wwwroot/_wasm/ and served as static files above.
// No special middleware needed — UseStaticFiles() handles them automatically.

app.UseRouting();
app.UseCors("WasmPolicy");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ─────────────────────────────────────────────────────────

// Root "/" → redirect to GP login (matches original site behaviour)
app.MapGet("/", () => Results.Redirect("/login"));

// ── MVC controllers (AuthController uses attribute routing) ────────
app.MapControllers();

// ── Razor Pages (About, Contact, PrivacyPolicy, Terms, etc.) ───────
app.MapRazorPages();

app.Run();
