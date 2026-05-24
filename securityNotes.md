# Security notes — component library offline bypass

**Status:** Development / local design-system use only  
**Action before production:** Remove or gate this implementation (see checklist below).

---

## What this bypass does

When the API is unreachable or the user is not signed in, the app still allows access to **public component-library story pages** without:

- loading an antiforgery token from the server, or
- requiring an authenticated session in `MainLayout`.

If the token fetch fails on a non–story route, the client **redirects to** `/componentlibrary-button` instead of blocking with an error.

### Affected routes (public stories only)

| Route | Page |
|-------|------|
| `/componentlibrary-button` | Button story |
| `/componentlibrary-selects` | Selects story |
| `/componentlibrary-formcontainer` | Form container story |
| `/componentlibrary-alert` | Alert story |
| `/componentlibrary-snackbar` | Snackbar story |

`/authorization-tests` and all other app routes are **not** part of this bypass (admin/auth rules still apply when the server is available).

---

## Why this is unsafe for production

1. **Antiforgery skipped** — Story URLs render without a valid request token; any later POST from those sessions may behave differently or weaken CSRF expectations.
2. **Anonymous shell access** — Unauthenticated users can use the full layout (nav, theme) on story routes without going through login.
3. **Fail-open on API errors** — Network/API failures no longer block the app; users are sent to the component library instead of seeing a security bootstrap error.
4. **Defence in depth removed** — Production should fail closed: no token and no auth means no app surface (or an explicit public-only login/marketing site).

---

## Files to change or remove before production

### 1. Delete (if no longer needed)

- `Client/Routes/ComponentLibraryRoutes.cs`

### 2. Restore strict antiforgery bootstrap

**File:** `Client/Components/Authorization/AntiforgeryBootstrap.razor`

Remove:

- `@using Client.Routes` and `@inject NavigationManager Navigation`
- Early return when `ComponentLibraryRoutes.IsPublicStoryRoute(Navigation)`
- `ProceedWithoutToken()` and its redirect / `_ready = true` without a token

Restore original behavior:

- Always call `LoadTokenAsync()` on startup
- On empty token or exception, set `_error` and **do not** render `@ChildContent` until retry succeeds

### 3. Restore strict layout auth

**File:** `Client/Layout/MainLayout.razor`

Remove:

- `@using Client.Routes`
- `isComponentLibraryStory` checks
- Conditional skip of `AuthService.RefreshClientAuthStateAsync()`
- Exception that allows `_shellReady` without authentication

Restore original behavior:

- Always `await AuthService.RefreshClientAuthStateAsync()`
- If not authenticated → `Navigation.NavigateTo("/login")` and return (no shell)

---

## Optional: environment-gated compromise

If stories must stay reachable in staging but not production, wrap bypass logic in configuration instead of deleting source:

```json
// appsettings.Development.json
{
  "ComponentLibrary": {
    "AllowOfflinePublicAccess": true
  }
}
```

```json
// appsettings.Production.json
{
  "ComponentLibrary": {
    "AllowOfflinePublicAccess": false
  }
}
```

Only call `ComponentLibraryRoutes` / `ProceedWithoutToken()` when `AllowOfflinePublicAccess` is `true`. **Production must set this to `false`.**

---

## Temporary offline theme toggle (development)

Theme defaults to **light** and is stored in **`localStorage`** (`app-theme-preference`) so the navbar switch works without the API.

**Before production:** decide whether to restore server-only theme settings, OS `prefers-color-scheme` bootstrap in `index.html`, and signed-in-only persistence. See `Client/wwwroot/js/theme.js`, `ThemeHandler.cs`, and `TopNavBar.razor.cs`.

---

## Pre-production checklist

- [ ] Remove or disable `ComponentLibraryRoutes` bypass (delete file or gate with config).
- [ ] `AntiforgeryBootstrap` requires a valid token before rendering the app.
- [ ] `MainLayout` requires authentication for all routes using that layout.
- [ ] Confirm component-library `@page` routes are not needed publicly in production (or protect with `[Authorize]` / server-side policies if they are).
- [ ] Review offline theme `localStorage` behavior (`theme.js`, `ThemeHandler`, `TopNavBar`).
- [ ] Smoke-test: API down → user sees error or login, **not** anonymous component library.
- [ ] Smoke-test: unauthenticated user → redirected to `/login` on all protected routes.

---

## Git reference (implementation introduced for local component-library work)

Search the repo for **`security risk`** (demo-only bypasses) or:

- `ComponentLibraryRoutes`
- `ProceedWithoutToken`
- `IsPublicStoryRoute`
- `isComponentLibraryStory`

Delete this file once the bypass has been removed from production builds.
