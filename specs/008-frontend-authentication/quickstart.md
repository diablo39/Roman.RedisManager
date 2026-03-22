# Quickstart: Frontend Authentication

**Feature**: 008-frontend-authentication  
**Date**: 2026-03-22

## What This Feature Does (explained simply)

Think of the app like a house. Right now, the front door is wide open — anyone
can walk in and look around. This feature adds a lock to the door.

1. **The lock** — A route guard that checks every visitor. If you don't have a
   key (access token), you get sent to the login page.
2. **The login page** — A simple screen that says "Pick a door to sign in" and
   shows buttons for each identity provider (like Microsoft or Google).
3. **Getting your key** — When you pick a provider, you're sent to their website
   to prove who you are. They send you back with a key (access token).
4. **Using your key** — Every time the app asks the server for data (like the
   list of Redis servers), it shows the key. The server checks it and lets the
   request through.

## How the Pieces Fit Together

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  login.vue   │────►│  auth store  │────►│  oidc-client  │
│  (pick door) │     │  (remembers) │     │  (handles key)│
└──────────────┘     └──────────────┘     └───────┬───────┘
                                                  │
                              signinRedirect()    │
                              ◄───────────────────┘
                                                  │
                     ┌────────────────────────────▼──────┐
                     │  Identity Provider (e.g. Microsoft)│
                     └────────────────────────────┬──────┘
                                                  │
                              redirect back       │
                     ┌────────────────────────────▼──────┐
                     │  callback.vue                      │
                     │  (processes the key, goes home)    │
                     └───────────────────────────────────┘
```

## Files You Will Touch

| File                           | What it does                              |
| ------------------------------ | ----------------------------------------- |
| `src/api/authentication.ts`    | Fetches the provider list from the server |
| `src/api/config.ts`            | Adds the access token to API requests     |
| `src/stores/authentication.ts` | Remembers providers, errors, login state  |
| `src/pages/login.vue`          | The "pick a door" screen                  |
| `src/pages/login/callback.vue` | Processes the key after redirect          |
| `src/layouts/login.vue`        | Minimal layout for login pages            |
| `src/router/auth.ts`           | Lists which pages are public              |
| `src/router/authGuard.ts`      | The lock on every page                    |
| `src/router/index.ts`          | Registers the lock                        |
| `src/api/redisServers.ts`      | Shows the key when asking for Redis data  |

## Step-by-Step Overview

### Step 1: Install the library

```bash
npm install oidc-client-ts
```

### Step 2: Create the API module

`src/api/authentication.ts` — one function: `getAuthenticationBootstrap()`.
It calls `GET /api/authentication/bootstrap` and returns the result.

### Step 3: Create the Pinia store

`src/stores/authentication.ts` — holds providers, loading state, errors.
Has actions: `loadBootstrap()`, `startSignIn(providerKey)`, `handleCallback()`,
`ensureSessionValid()`.

### Step 4: Create the login page

`src/pages/login.vue` — calls `loadBootstrap()` on mount, shows a button for
each provider. On click, calls `startSignIn(providerKey)`.

### Step 5: Create the callback page

`src/pages/login/callback.vue` — calls `handleCallback()` on mount, then
navigates to the return URL or home.

### Step 6: Create the login layout

`src/layouts/login.vue` — just `<v-main><router-view /></v-main>`, no sidebar.

### Step 7: Add the route guard

`src/router/authGuard.ts` — `router.beforeEach` that checks for a valid user.
If missing, redirect to `/login` with `returnUrl` query param.

### Step 8: Add auth headers to API calls

`src/api/config.ts` — add `getAuthHeaders()` that returns
`{ Authorization: 'Bearer <token>' }`. Update `redisServers.ts` to include
these headers in every `fetch()`.

### Step 9: Wire it up

Register the guard in `src/router/index.ts`.

## Verification Checklist

- [ ] `npm install` completes without errors
- [ ] `npm run lint` passes
- [ ] `npm run type-check` passes
- [ ] Opening `/` while signed out redirects to `/login`
- [ ] `/login` shows provider buttons when bootstrap returns available
- [ ] `/login` shows an error message when bootstrap returns unavailable
- [ ] Clicking a provider redirects to the external identity provider
- [ ] Returning from the provider lands on `/login/callback` and then redirects home
- [ ] Redis server list loads with an `Authorization` header visible in dev tools
- [ ] Opening `/login` while signed in redirects away from login
