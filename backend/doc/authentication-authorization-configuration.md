# Authentication and Authorization Configuration

> 🚀 **Goal:** Anyone — even a 10-year-old with a bit of help — should be able to set up secure login for Roman.RedisManager. No code changes needed. You only edit a JSON file and include a pass with every request.

Think of Roman.RedisManager as a **building with a locked door**. To get in, you need a **visitor pass** (called a *token* — a short piece of text that proves who you are). This guide shows you how to get a pass and teach the building which passes to accept.

---

## What you will need before you start

- 🖥️ Roman.RedisManager is already running somewhere (on your computer or a server)
- 🏢 An account with a login provider — one of: **Microsoft Entra ID**, **Google**, or another OpenID Connect server
- 💻 A terminal / command line (Windows PowerShell, macOS Terminal, Linux bash)
- 🌐 Internet access to reach the login provider's website

> 🔒 A token is like a visitor pass. If you drop it, someone else can use it to enter as you. Never paste tokens into emails, chat messages, or source code files.

---

## Quick overview — the five steps

Here is the whole journey at a glance. Each step is explained in detail below.

1. **Tell your login provider about this app** — so it knows to issue passes for it.
2. **Copy the three values you get** (Authority, Client ID, Client Secret) into `appsettings.json`.
3. **Decide who gets which role** — by mapping provider claims to app roles in `appsettings.json`.
4. **Restart the API** so it reads your new settings.
5. **Get a token and test it** with a single command.

---

## 1. Register the app with your login provider

Before the login provider will issue tokens for Roman.RedisManager, it needs to know the app exists. This is called *registering an application* — it takes about five minutes and you only do it once.

### 1.1 Entra ID (Microsoft) — step by step

1. **Open** [portal.azure.com](https://portal.azure.com) and sign in.
2. In the top search bar type *App registrations* and click it.
3. Click **New registration**.
4. Fill in:
   - *Name*: anything you like, e.g. `Roman RedisManager`
   - *Supported account types*: pick **"Accounts in this organizational directory only"** (you can change this later)
   - *Redirect URI*: leave blank (this API does not need one)
5. Click **Register**.
6. You are now on the app's overview page. Copy and save:
   - **Application (client) ID** — a long code like `11111111-aaaa-bbbb-cccc-222222222222`
   - **Directory (tenant) ID** — another long code on the same page
7. Create a secret: click **Certificates & secrets** → **New client secret** → give it a name → click **Add**. **Copy the secret value immediately** — it is only shown once.

> ✅ **You now have three things you need:**
> - `Authority` = `https://login.microsoftonline.com/<Directory-tenant-ID>/v2.0`
> - `ClientId` = the Application (client) ID you copied
> - `ClientSecret` = the secret value you copied

---

### 1.2 Google — step by step

1. **Open** [console.cloud.google.com](https://console.cloud.google.com/apis/credentials) and sign in.
2. Click **Create credentials** → **OAuth 2.0 Client ID**.
3. Set *Application type* to **Web application**.
4. Under *Authorized redirect URIs* add `https://localhost` (just a placeholder — the API doesn't use it).
5. Click **Create**. Copy the **Client ID** and **Client secret** from the dialog that appears.

> ✅ **You now have three things you need:**
> - `Authority` = `https://accounts.google.com`
> - `ClientId` = the Client ID you copied
> - `ClientSecret` = the Client secret you copied

---

### 1.3 Generic OIDC server — step by step

*OpenID Connect (OIDC)* is a standard login protocol used by many corporate systems (Okta, Keycloak, Auth0, etc.).

1. Ask your OIDC administrator for:
   - The **issuer URL** — it ends with something like `/realms/myrealm` or `/oauth2/default`
   - A **Client ID** and **Client Secret** for this application
2. Confirm you can open `<issuer-url>/.well-known/openid-configuration` in a browser — it should return a JSON page. If it does, the URL is correct.

> ✅ **You now have three things you need:**
> - `Authority` = the issuer URL
> - `ClientId` = provided by your OIDC admin
> - `ClientSecret` = provided by your OIDC admin

---

## 2. Edit `appsettings.json`

Open the file `src/Roman.RedisManager.Web/appsettings.json` (or a per-environment override file). You will add or update two sections inside `"Security"`.

### 2.1 Tell the app about your login provider

This block tells the app: *"I trust tokens that come from this provider."*

```json
"Security": {
  "Authentication": {
    "Providers": [
      {
        "ProviderKey": "entra",
        "DisplayName": "Entra ID",
        "Enabled": true,
        "Kind": "EntraId",
        "Authority": "https://login.microsoftonline.com/YOUR-TENANT-ID/v2.0",
        "ClientId": "YOUR-CLIENT-ID",
        "ClientSecret": "YOUR-SECRET-VALUE"
      }
    ]
  }
}
```

Field guide:

| Field | What to put here |
|---|---|
| `ProviderKey` | A short nickname you invent, e.g. `"entra"` or `"google"`. Used in other config sections to refer to this provider. |
| `DisplayName` | Human-readable label, shown in logs. |
| `Kind` | Must be exactly `"EntraId"`, `"Google"`, or `"GenericOidc"`. |
| `Authority` | The issuer URL from section 1. |
| `ClientId` | The Client ID from section 1. |
| `ClientSecret` | The secret from section 1. |

> 💡 You can add more than one provider by adding more objects inside the `Providers` array. Each needs a unique `ProviderKey`.

---

### 2.2 Define roles and permissions

This block tells the app: *"These are the roles that exist, and these are the things each role is allowed to do."*

```json
"Authorization": {
  "Roles": [
    { "RoleName": "redis-reader", "Description": "Can read data" },
    { "RoleName": "editor",       "Description": "Can write data" },
    { "RoleName": "admin",        "Description": "Full access" }
  ],
  "RoleClaimMappings": [
    // see section 2.3 below for step-by-step setup
    {
      "RoleName": "redis-reader",
      "ProviderKey": "entra",
      "ClaimKey": "roles",
      "AllowedValues": [ "Reader" ],
      "MatchMode": "Any"
    }
  ],
  "Permissions": {
    "Global": [
      { "Action": "DeleteKey", "AllowedRoles": [ "editor", "admin" ] }
    ]
  }
}
```

> 🎯 Start with just the roles you need. You can add or remove roles later without touching any code — just edit this file and restart.

---

### 2.3 RoleClaimMappings — who gets which role

#### What is a RoleClaimMapping?

Think of it like a guest list at a school party.

- The **party** is Roman.RedisManager.
- Every guest (user) arrives with a **badge** (a JWT token — a piece of text that proves who you are) that lists facts about them — like which clubs they belong to or which jobs they have.
- A `RoleClaimMapping` is one rule that says: *"If the badge says X, let this person in as a Y."*

Each rule has four parts:

| Field | What it means | Example |
|---|---|---|
| `RoleName` | The role inside this app the person will get | `"redis-reader"` |
| `ProviderKey` | Which login provider issued the badge | `"entra"` |
| `ClaimKey` | Which section of the badge to look at | `"roles"` or `"groups"` |
| `AllowedValues` | What the badge must say in that section | `["Reader"]` |

If the badge's `ClaimKey` section contains at least one of the `AllowedValues`, the person is granted that `RoleName`.

---

#### Which approach should I use with Entra ID?

You have two options. **App Roles is recommended** — it is purpose-built for app permissions and is simpler to maintain.

| | **Approach A: App Roles** ✅ recommended | **Approach B: Security Groups** |
|---|---|---|
| **Where you set it up** | App Registration → App roles tab | Any existing Entra ID Security Group |
| **What goes in `ClaimKey`** | `"roles"` | `"groups"` |
| **What goes in `AllowedValues`** | Role name you chose, e.g. `"Reader"` | Group Object ID — a long GUID like `"a1b2c3d4-..."` |
| **Best for** | Permissions that belong to this app only | Reusing groups that already exist in your organization |

---

#### Approach A: App Roles (recommended) — step by step

1. **Open** [portal.azure.com](https://portal.azure.com) → search for *App registrations* → click your app.
2. Click **App roles** in the left menu → **Create app role**.
3. Fill in:
   - *Display name*: anything readable, e.g. `Redis Reader`
   - *Allowed member types*: `Users/Groups`
   - *Value*: the string that will appear in the token, e.g. `Reader` (no spaces)
   - *Description*: short note
   - Check *Enable this app role*
   - Click **Apply**.
4. Now assign the role to a user or group:
   - Search for *Enterprise applications* → click your app.
   - Click **Users and groups** → **Add user/group**.
   - Pick the user (or group) → pick the role (`Redis Reader`) → click **Assign**.
5. Edit `appsettings.json`:
   ```json
   {
     "RoleName": "redis-reader",
     "ProviderKey": "entra",
     "ClaimKey": "roles",
     "AllowedValues": [ "Reader" ],
     "MatchMode": "Any"
   }
   ```
6. Restart the API.
7. Get a new token (`az account get-access-token ...`) and paste it into [jwt.ms](https://jwt.ms). Look for `"roles": ["Reader"]` in the decoded claims. If you see it, the setup is correct.

> 💡 The value in `AllowedValues` must match the **Value** field you typed in step 3 exactly (case-insensitive).

---

#### Approach B: Security Groups — step by step

1. **Open** [portal.azure.com](https://portal.azure.com) → search for *Groups* → click the group you want to use.
2. On the **Overview** page, copy the **Object ID** — it looks like `a1b2c3d4-0000-0000-0000-000000000000`. A GUID is just a long unique code that Azure uses to identify things.
3. Go to *App registrations* → your app → **Manifest** (left menu).
4. Find the line `"groupMembershipClaims": null` and change it to:
   ```json
   "groupMembershipClaims": "SecurityGroup"
   ```
   Click **Save**.
5. Edit `appsettings.json`:
   ```json
   {
     "RoleName": "redis-reader",
     "ProviderKey": "entra",
     "ClaimKey": "groups",
     "AllowedValues": [ "a1b2c3d4-0000-0000-0000-000000000000" ],
     "MatchMode": "Any"
   }
   ```
   Replace the GUID with the Object ID you copied in step 2.
6. Restart the API.
7. Get a new token and paste it into [jwt.ms](https://jwt.ms). Look for `"groups": ["a1b2c3d4-..."]`. If you see your GUID there, the setup is correct.

> ⚠️ If the user belongs to more than ~200 groups, Azure may leave the `groups` claim out of the token entirely. If that happens, switch to Approach A.

---

#### MatchMode: Any vs All

`MatchMode` controls how `AllowedValues` are checked.

| MatchMode | Meaning | Use when |
|---|---|---|
| `"Any"` (default) | Access if the badge contains **at least one** of the listed values | Granting a role via any one of several groups/roles |
| `"All"` | Access only if the badge contains **every** listed value | Requiring membership in two groups simultaneously |

Example — a user must belong to *both* a `security-team` group and a `redis-approved` group to get admin:

```json
{
  "RoleName": "admin",
  "ProviderKey": "entra",
  "ClaimKey": "groups",
  "AllowedValues": [
    "security-team-object-id-guid",
    "redis-approved-object-id-guid"
  ],
  "MatchMode": "All"
}
```

---

### 2.4 Save and restart

The API reads `appsettings.json` once when it starts up — it does not watch for changes while running. So every time you edit the file, you need to restart the API to apply the new settings.

How to restart depends on how you run it:

- **Running locally with `dotnet run`**: press `Ctrl+C` to stop, then run the command again.
- **Running in Docker**: `docker restart <container-name>`.
- **Running as a service**: use whatever command your server uses to restart services.

> 💡 After restarting, try a curl request (section 4) straight away to confirm the new config loaded correctly.

---

## 3. Get a visitor pass (bearer token)

A *bearer token* is like a show-this-pass-and-you-get-in card. You present it with every API request. "Bearer" just means "whoever holds this token is who they say they are."

> ⚠️ **Common mistake:** People try to use their personal browser login session as a token. That does not work. You need a token issued specifically for the Client ID you registered in section 1.

### 3.1 Entra ID — using Azure CLI

The Azure CLI (`az`) is a free command-line tool from Microsoft. If you don't have it, [download it here](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli).

1. Open your terminal and run:
   ```powershell
   az login
   ```
   A browser window opens. Sign in with the account that has been assigned a role in section 2.3.

2. Get the token:
   ```powershell
   az account get-access-token --resource api://YOUR-CLIENT-ID --query accessToken -o tsv
   ```
   Replace `YOUR-CLIENT-ID` with the Application (client) ID from section 1.1.

3. The command prints a long string of letters and numbers. That is your token. Copy it.

> 💡 To check what's inside the token, paste it into [jwt.ms](https://jwt.ms). You should see the `roles` or `groups` claim you configured.

---

### 3.2 Google — using gcloud CLI

The gcloud CLI is Google's command-line tool. If you don't have it, [download it here](https://cloud.google.com/sdk/docs/install).

1. Open your terminal and run:
   ```bash
   gcloud auth login
   ```
   A browser window opens. Sign in with your Google account.

2. Get the token:
   ```bash
   gcloud auth print-access-token
   ```

3. The command prints a long string. That is your token. Copy it.

> 💡 Google tokens include a `scope` claim with `https://www.googleapis.com/auth/userinfo.email` by default. Make sure your `RoleClaimMappings` use `"ClaimKey": "scope"` (or whatever claim your Google setup provides).

---

### 3.3 GitHub Actions (OIDC)

> ⚠️ **GitHub is different from other providers.** GitHub does not let you create a token from your personal computer. The only way to get a GitHub OIDC token is from *inside a running GitHub Actions workflow* — the workflow itself automatically gets one.

If you are using GitHub Actions:
- Use the [`azure/login`](https://github.com/azure/login) action, which puts the token into `steps.azure_login.outputs.access-token`.
- Pass that value to your API call the same way as in section 4 below.

You cannot test this with curl on your local machine — you must run a workflow.

---

### 3.4 Generic OIDC

Every OIDC server has its own way to issue tokens. Common options:

- **Postman** (beginner-friendly): use the *Authorization* tab → OAuth 2.0 → fill in Client ID, Secret, and Token URL.
- **curl**: use the client credentials flow (ask your OIDC admin for the exact command).

The token you get must be a standard JWT (it looks like three Base64 strings separated by dots). Paste it into [jwt.ms](https://jwt.ms) to verify it contains the claims you mapped in section 2.3.

---

## 4. Test with a real request

`curl` is a command-line tool that sends web requests. It comes pre-installed on Windows 10/11, macOS, and Linux.

1. Paste your token into this command (replace everything including the `< >`):
   ```bash
   curl -H "Authorization: Bearer <your-token>" \
        https://your-api-host/api/redis-servers
   ```

2. Two things can happen:

   - ✅ **It works** — you get back a JSON list of Redis servers. You are done!
   - ❌ **Error 401 (Unauthorized)** — the token was missing, expired, or came from a provider the app doesn't recognise. Get a fresh token and try again.
   - ❌ **Error 403 (Forbidden)** — the token is valid, but the role it maps to doesn't have permission for this action. Check your `RoleClaimMappings` and `Permissions` config.

> 💡 If you are not sure what your token contains, paste it into [jwt.ms](https://jwt.ms). Look for the `roles`, `groups`, or `scope` claim — those are what your mappings look at.

---

## 5. Something went wrong? — troubleshooting guide

Work through this checklist from top to bottom. Most problems are solved by step 3 or 4.

---

### The API returns 401 (Unauthorized)

This means the app did not accept the token at all.

- **Is the token expired?** Tokens are valid for a limited time (usually 1 hour). Run the `az login` / `gcloud auth` command again to get a fresh one.
- **Is the `ProviderKey` in your JSON the same in both the `Providers` section and the `RoleClaimMappings` section?** A typo (e.g. `"Entra"` vs `"entra"`) will break it. The match is case-insensitive, but check for extra spaces.
- **Is the `Authority` URL correct?** Copy it from section 1 again and compare carefully.

---

### The API returns 403 (Forbidden)

This means the token was accepted, but the user doesn't have permission for this action.

- Paste the token into [jwt.ms](https://jwt.ms). Does the `roles` or `groups` claim contain the value you put in `AllowedValues`?
- Is the `RoleName` in `RoleClaimMappings` spelled exactly the same as the one in `Permissions → AllowedRoles`?
- Did you restart the API after editing `appsettings.json`?

---

### The `roles` or `groups` claim is missing from the token

Some providers don't include these claims unless you specifically ask for them.

- **Entra ID App Roles:** make sure you assigned the user to the role in *Enterprise applications → Users and groups* (section 2.3, step 4). Get a new token after assigning.
- **Entra ID Groups:** make sure you set `"groupMembershipClaims": "SecurityGroup"` in the app manifest (section 2.3, Approach B step 4).
- **Any provider:** paste the token into [jwt.ms](https://jwt.ms) to see exactly what claims are present. The claim you put in `ClaimKey` must appear there.

---

### Quick pre-support checklist

Before asking anyone for help, run through these:

- [ ] I restarted the API after editing `appsettings.json`
- [ ] I got a **new** token after making changes (old tokens don't reflect new role assignments)
- [ ] I checked the token at [jwt.ms](https://jwt.ms) and can see the expected claim
- [ ] The `ProviderKey` value matches exactly between `Providers` and `RoleClaimMappings`
- [ ] The `AllowedValues` entry matches exactly what is in the token claim

---

## 6. Changing who can do what — no code needed

The whole permission system lives in `appsettings.json`. You never need to change code.

**Example scenario:** You just hired a new admin. Here's what to do:

1. In Azure portal (or your provider), assign the new user to the appropriate app role or group.
2. The user gets a new token — they are now recognized as an admin automatically. No config change needed for this case!

**Example scenario:** You want to restrict who can delete Redis keys.

1. Open `appsettings.json`.
2. Find the `Permissions → Global` section and change the `AllowedRoles` for `"Action": "DeleteKey"`.
3. Restart the API.
4. Test with curl.

That's it — no pull requests, no deployments (beyond restarting the API).

> 💡 In production you can inject these values via environment variables or a secrets manager (like Azure Key Vault) instead of editing the file directly, so you never store secrets in source control.

---

## 7. Want to go deeper?

The configuration format is fully documented in the spec files:

- `specs/001-oidc-role-authorization/identity-configuration.md` — every field in the `Authentication` section explained, with all allowed values.
- `specs/001-oidc-role-authorization/authorization-configuration.md` — the full list of `Action` names you can use in `Permissions`.

These are useful if you want to add a new provider type, write automation scripts, or understand why a particular setting exists.

---

You're done! 🎉

Anyone who has an account with your chosen login provider, and has been assigned the right role, can now use Roman.RedisManager.
