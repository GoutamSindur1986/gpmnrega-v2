# GP MNREGA v2 — .NET 8 Migration

## Project Structure

```
GpMnrega.sln
├── GpMnrega.Web/          ASP.NET Core 8 MVC + Razor Pages (main app)
├── GpMnrega.DataLayer/    Class library — Dapper repositories
└── GpMnrega.Wasm/         Blazor WebAssembly (PDF + subscription logic)
```

---

## Step 1 — Prerequisites

- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8
- Visual Studio 2022 v17.8+ or VS Code with C# Dev Kit
- SQL Server (existing DB — no schema changes needed)
- IIS with ASP.NET Core Module V2 installed

---

## Step 2 — Configure appsettings.json

Edit `GpMnrega.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "dbgpmnregadev": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
},
"Auth": {
  "JwtSecret": "GENERATE_32_CHAR_RANDOM_STRING_HERE",
  "ObfuscationKey": "YOUR_16_CHAR_KEY"
},
"Smtp": {
  "Password": "YOUR_SMTP_PASSWORD"
}
```

Generate JWT secret: `openssl rand -base64 32`

---

## Step 3 — Copy existing jQuery scripts

Copy ALL files from old project's `Scripts/` to `GpMnrega.Web/wwwroot/js/`:
- jquery-3.4.1.min.js
- pdfmake.js, vfs_fonts.js, pdflib.js
- moment.js
- modalpopup.js
- All Formtemplates/*.js
- db.js, mr.js, cashbook.js, etc.
- mragency.js, agencyhome.js

The new `gphome.js` and `wasm-bridge.js` are already in wwwroot/js/.

---

## Step 4 — Copy existing fonts/assets

Copy from old project:
- `fonts/tunga.ttf` → `GpMnrega.Web/wwwroot/fonts/`
- `Content/*.css` (keep for reference, Bootstrap 5 replaces most of it)
- `GPGuidelinefiles/` → `GpMnrega.Web/wwwroot/GPGuidelinefiles/`
- `gandhi-icon.ico` → `GpMnrega.Web/wwwroot/favicon.ico`

---

## Step 5 — Build and run

```bash
cd GpMnrega.sln directory
dotnet restore
dotnet build

# Run web project
cd GpMnrega.Web
dotnet run
```

---

## Step 6 — Publish to IIS

```bash
dotnet publish GpMnrega.Web -c Release -o ./publish

# Also publish Blazor WASM (outputs to wwwroot/_wasm automatically)
dotnet publish GpMnrega.Wasm -c Release
```

Then:
1. Copy `./publish` to IIS site folder
2. In IIS Manager: Application Pool → .NET CLR Version = "No Managed Code"
3. Ensure ASP.NET Core Module V2 is installed (comes with Hosting Bundle)

---

## API URL Migration Map

All old `.aspx` proxy calls map to new endpoints:

| Old URL | New URL |
|---------|---------|
| `/api/getworkdata.aspx` | `/api/proxy/getworkdata` |
| `/api/getNmrData.aspx` | `/api/proxy/getnmrdata` |
| `/api/getWageListData.aspx` | `/api/proxy/getwagelist` |
| `/api/getftoDetails.aspx` | `/api/proxy/getftodetails` |
| `/api/getForm8Data.aspx` | `/api/proxy/getform8data` |
| `/api/getAgencyworkdata.aspx` | `/api/proxy/getagencyworkdata` |
| `/api/getAgencyAsset.aspx` | `/api/proxy/getagencyasset` |
| `/api/getWorkAsset.aspx` | `/api/proxy/getworkasset` |
| `/api/getAgencyFto.aspx` | `/api/proxy/getagencyfto` |
| `/api/jobCardHH.aspx` | `/api/proxy/jobcardhh` |
| `/api/getcatwisejobcards.aspx` | `/api/proxy/getcatwisejobcards` |
| `/pullgpcodes.aspx` | `/pullgpcodes` |

---

## Pages completed

| Old | New |
|-----|-----|
| `login.aspx` | `/Views/Auth/Login.cshtml` |
| `DeptLogin.aspx` | `/Views/Auth/Login.cshtml` (same view, tab switch) |
| `Auth/Home.aspx` | `/Pages/Auth/Home.cshtml` |
| `Auth/Cashbook.aspx` | `/Pages/Auth/Cashbook.cshtml` |
| `Auth/PaySub.aspx` | `/Pages/Auth/Subscription.cshtml` |
| `Auth/Sales.aspx` | `/Pages/Offers.cshtml` |
| `AuthAgency/depthome.aspx` | `/Pages/AuthAgency/DeptHome.cshtml` |
| `Emailverification.aspx` | `/Views/Auth/EmailVerification.cshtml` |
| `passwordReset.aspx` | `/Views/Auth/PasswordReset.cshtml` |
| All `/api/*.aspx` | `/api/proxy/*` (ProxyController.cs) |
| `/pullgpcodes.aspx` | `/pullgpcodes` (GpCodesController.cs) |

## Remaining for Phase 2

- `Auth/PaymentResponse.aspx` → `/api/payment/response` (partially done in PaymentController)
- `AuthAgency/PaySubDept.aspx` → reuse `/Pages/Auth/Subscription.cshtml` (UserType claim handles both)
- `AuthAgency/PaymentResponseDept.aspx` → same PaymentController response endpoint
- All `Registers/*.aspx` template pages → existing jQuery scripts unchanged, served via proxy
- `About.aspx`, `Contact.aspx`, `Privacypolicy.aspx`, `termsNconditions.aspx` → simple Razor pages
- Add real AI backend to chatbot (ChatController → Claude/OpenAI API)
- WebRTC call feature (dedicated Phase 3 session)

---

## Cookie changes from old to new

| Old | New | Notes |
|-----|-----|-------|
| `.S3KN_AUTH2` (FormsAuth) | `.S3KN_AUTH2` (Cookie Auth) | HttpOnly, Secure |
| `UserData` cookie (readable) | Claims in auth cookie | HttpOnly |
| `test` / `test1` cookies | Claims in auth cookie | HttpOnly |
| `UserType` cookie (readable) | Claim in auth cookie | HttpOnly |
| No JWT | `SubToken` JWT | Not HttpOnly — WASM reads it |

---

## Adding real AI to chatbot (Phase 3)

Replace `getBotResponse()` in `chatbot.js` with:

```javascript
async function getBotResponse(msg) {
  const resp = await fetch('/api/chat/message', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ message: msg, history: messageHistory })
  });
  const data = await resp.json();
  return data.reply;
}
```

Then create `Controllers/ChatController.cs` that calls Claude/OpenAI API server-side.
