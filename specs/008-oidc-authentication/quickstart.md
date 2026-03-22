# Quickstart: OIDC Authentication Feature

## 1. Install dependencies

Run commands from the frontend root directory.

- npm install oidc-client-ts
- npm install -D vitest @vue/test-utils jsdom

## 2. Implement feature slices

Path convention in this guide uses repository-root paths.

1. Add authentication API client for bootstrap retrieval:
   - frontend/src/api/authentication.ts
2. Add authentication store for session state and OIDC orchestration:
   - frontend/src/stores/authentication.ts
3. Add sign-in page to show provider choices and state feedback:
   - frontend/src/pages/login.vue
4. Add callback route page to process OIDC redirect responses:
   - frontend/src/pages/auth/callback.vue
5. Add global router auth guard:
   - frontend/src/router/index.ts

## 3. Guard behavior contract

- Public routes: /login and callback path.
- Protected routes: all other pages/screens.
- Unauthenticated access to protected route redirects to /login with returnUrl.
- Authenticated user navigating to /login redirects to returnUrl or default route.

## 4. Validation workflow

1. Run quality gates:
   - npm run lint
   - npm run type-check
   - npm run build
2. Run authentication tests:
   - npx vitest run
3. Manual smoke checks:
   - Sign-in page loads and fetches providers from GET /api/authentication/bootstrap.
   - Selecting provider triggers redirect flow.
   - Direct navigation to protected route while logged out redirects to /login.
   - After callback success, user reaches intended protected destination.
   - Bootstrap unavailable state shows clear non-technical message with no active sign-in actions.
   - Session expiration during protected navigation redirects back to /login with returnUrl preserved.

## 5. Measurable outcome validation

- SC-003 first-attempt sign-in start rate:
  - Run a 20-attempt UAT checklist and confirm at least 18 attempts successfully start provider redirect on first action.
- SC-006 support-ticket trend:
  - Capture support ticket count tagged `auth-login-discovery` for 30 days pre-release and 30 days post-release.
  - Confirm post-release count is at least 25% lower than pre-release baseline.

## 6. Performance checks

- Measure sign-in provider readiness timing in devtools network/performance tools.
- Confirm provider list renders within the 2s p95 target under normal network conditions.
- Verify no protected content is painted before redirect when unauthenticated.
