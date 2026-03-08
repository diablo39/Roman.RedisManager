# Authentication and Authorization Configuration

> 🚀 **Goal:** let people age 10 and up configure secure login for Roman.RedisManager using tokens. No code changes, just edit a JSON file and supply a token with every request.

---

## Quick overview

1. **Pick a login provider** – Entra ID (Microsoft), Google, or your own OpenID Connect (OIDC) server.
2. **Register an application there** to get a Client ID (and secret).
3. **Paste the details into `appsettings.json`.**
4. **Tell the app what internal roles exist and how provider claims map to them.**
5. **Restart the API and try a curl command with a bearer token.**

This document walks through each step and shows simple examples you can follow exactly.

---

## 1. Before you begin

- You need an existing Roman.RedisManager deployment (the API running on some host).
- You also need a login provider account (Microsoft Entra, Google Cloud, or another OIDC server).
- A working command line with `curl` (Windows PowerShell, macOS Terminal, etc.).

> 🔒 Tokens are like keys. Whoever has the token can act as you. Treat them carefully and don’t check them into source control.

---

## 2. Register an application with the provider

| Provider | What to register | Notes |
|----------|------------------|-------|
| **Entra ID (Microsoft)** | App registration in the [Azure portal](https://portal.azure.com) | Choose “Accounts in this organizational directory only” or “Multitenant” as needed. Use any redirect URI (not used by this API). Copy the **Application (client) ID** and **Directory (tenant) ID**. Create a client secret. |
| **Google** | OAuth 2.0 Client ID in [Google Cloud Console](https://console.cloud.google.com/apis/credentials) | Application type = Web application. Redirect URI can be `https://localhost`. Copy the **Client ID** and **Client secret**. |
| **Generic OIDC** | Ask your OIDC admin for the issuer URL, client ID/secret | You must know the issuer’s `.well-known/openid-configuration` URL. |

> 📌 **Common mistake:** People try to use their personal login token. That won’t work. You need a token issued for the client ID you registered.

After registration you’ll have:
- **Authority** (issuer URL) – e.g. `https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0`
- **ClientId** – a GUID-like string.
- **ClientSecret** – long secret string (keep it secret!).

---

## 3. Edit configuration

Open `src/Roman.RedisManager.Web/appsettings.json` (or environment-specific override) and add or update these sections.

### 3.1 Authentication providers

```json
"Security": {
  "Authentication": {
    "Providers": [
      {
        "ProviderKey": "entra",           // just a short name you pick
        "DisplayName": "Entra ID",
        "Enabled": true,
        "Kind": "EntraId",              // "Google" or "GenericOidc"
        "Authority": "https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0",
        "ClientId": "your-client-id-guid",
        "ClientSecret": "your-secret-value"
      }
      // you can add more providers here (google, generic, ...)
    ]
  },
  "Authorization": {
    "Roles": [
      { "RoleName": "redis-reader", "Description": "Can read data" },
      { "RoleName": "editor",       "Description": "Can write data" },
      { "RoleName": "admin",        "Description": "Full access" }
    ],
    "RoleClaimMappings": [
      {
        "RoleName": "redis-reader",
        "ProviderKey": "entra",
        "ClaimKey": "roles",         // or "groups", "scope", etc.
        "AllowedValues": [ "Reader" ],
        "MatchMode": "Any"
      }
      // add mappings for editor/admin as needed
    ],
    "Permissions": {
      "Global": [
        { "Action": "DeleteKey", "AllowedRoles": [ "editor", "admin" ] }
        // see the codebase for a full list of actions
      ]
    }
  }
}
```

> 🎯 Only the parts shown above are required to get started. You can ignore group-specific overrides unless you run multiple Redis clusters.

### 3.2 Save and restart

Every time you change `appsettings.json` (or the environment variable that points to it), you must restart the API so it picks up the new values.

---

## 4. Get a bearer token

You must obtain a JWT issued by the same authority you configured.

### 4.1 Entra ID example using Azure CLI

```powershell
# login and get token (requires az cli)
az login
az account get-access-token --resource api://your-client-id-guid --query accessToken -o tsv
```

### 4.2 Google example using gcloud

```bash
gcloud auth login
gcloud auth print-access-token
```

*(Google tokens contain `https://www.googleapis.com/auth/userinfo.email` scope by default.)*

### 4.3 GitHub Actions (OIDC) **note**

GitHub does **not** let you mint a token using your personal user account. The only way to obtain a token is from within a **GitHub Actions workflow** using the built‑in `${{ steps.azure_login.outputs.access-token }}` or the [`azure/login`](https://github.com/azure/login) action. That token is presented to the API the same way as above.

> ⚠️ You cannot paste a GitHub login page token into curl and expect it to work. It must come from an action run.

### 4.4 Generic OIDC

Use whatever mechanism your OpenID provider offers (Postman, curl, device code flow). The token must be a standard JWT containing the claims you mapped earlier (`roles`, `groups`, etc.).

---

## 5. Try an API request with curl

Replace `<your-token>` with the token string you just obtained.

```bash
curl -H "Authorization: Bearer <your-token>" \
     https://your-api-host/api/redis-servers
```

A successful call returns JSON; a 401/403 means the token was missing, invalid, or didn’t map to any allowed role for that action.

> 🛠 Tip: copy the token into https://jwt.ms to inspect its claims.

If you see `"roles":["redis-reader"]` but the call still fails, double‑check your `RoleClaimMappings` and `Permissions:Global` configuration.

---

## 6. Troubleshooting

1. **Getting 401 (unauthorized)?**
   - Token expired – get a new one.
   - ProviderKey in JSON doesn’t match the one you used when mapping.
   - Missing `AllowedValues` match.
2. **Getting 403 (forbidden)?**
   - The token is valid but the role isn’t permitted for that action.
   - Verify the `Global` permission list or group override.
3. **Claims not present?**
   - Some providers don’t include groups/roles by default; enable them in the app registration.
   - Use a token explorer (jwt.ms) to inspect the raw JWT.
4. **GitHub Actions token not working?**
   - Remember that GitHub only issues tokens inside workflows. See the section above.

---

## 7. Making changes later

To adjust who can do what you don’t need code changes:

1. Edit the same JSON sections (`Roles`, `RoleClaimMappings`, `Permissions`, or per-group overrides).
2. Restart the API.
3. Test with curl again.

Those edits can also be automated via environment variables or a secrets manager in production.

---

## 8. Deep dive reference

For more detailed information about the configuration format and upgrade guidance, see the spec documents:

- `specs/001-oidc-role-authorization/identity-configuration.md`  (maps 1:1 to the JSON above)
- `specs/001-oidc-role-authorization/authorization-configuration.md` (permission actions list)

These files are useful if you want to add new provider kinds or write automation scripts.

---

You're done! 🎉

Now anyone who can get a token from your chosen provider and has the right claims can log in and use Roman.RedisManager.
