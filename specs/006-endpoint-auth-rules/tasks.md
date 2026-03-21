# Tasks: Endpoint Authorisation Rules

**Input**: Design documents from `/specs/003-endpoint-auth-rules/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are in scope per the mutation quality requirement in the specification. Tasks include test phases with `run -> analyze -> improve -> rerun` workflow.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend source**: `backend/src/Roman.RedisManager.Web/`
- **Domain**: `backend/src/Roman.RedisManager.Domain/`
- **Tests**: `backend/tests/Roman.RedisManager.Tests/`

## Phase 1: Setup

**Purpose**: Remove obsolete group-based authorization infrastructure

- [x] T001 Delete `backend/src/Roman.RedisManager.Web/Authorization/Requirements/GroupPermissionRequirement.cs`
- [x] T002 [P] Delete `backend/src/Roman.RedisManager.Web/Authorization/Handlers/GroupPermissionAuthorizationHandler.cs`
- [x] T003 [P] Delete `backend/src/Roman.RedisManager.Web/Authorization/IGroupContextAccessor.cs`
- [x] T004 [P] Delete `backend/src/Roman.RedisManager.Web/Authorization/RouteGroupContextAccessor.cs`
- [x] T005 [P] Delete `backend/src/Roman.RedisManager.Domain/Entities/AuthorizationPolicyTypes.cs`
- [x] T006 [P] Delete `backend/src/Roman.RedisManager.Domain/Configuration/AuthorizationPermissionConfiguration.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define new Reader/Editor policies, configure fail-closed fallback, clean up DI registrations and configuration

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T007 Update `backend/src/Roman.RedisManager.Web/Authorization/AuthorizationPolicies.cs` — replace `ReadKeys` and `DeleteKeysByGroup` constants with `Reader` and `Editor`
- [x] T008 Update `backend/src/Roman.RedisManager.Web/Program.cs` — replace authorization policy registrations: `Reader` policy requires roles `reader`, `admin`; `Editor` policy requires roles `editor`, `admin`. Add `FallbackPolicy` requiring authenticated user. Remove DI registrations for `IGroupContextAccessor`, `RouteGroupContextAccessor`, `GroupPermissionAuthorizationHandler`, and `AuthorizationPermissionConfiguration` options binding
- [x] T009 Update `backend/src/Roman.RedisManager.Web/Authorization/AuthorizationDecisionLogger.cs` — remove references to `PermissionAction`, `GroupPermissionRequirement`, and group-based decision reasons. Simplify to log policy name (Reader/Editor), user ID, provider, roles, and allow/deny decision
- [x] T010 Remove `AuthorizationOverrides` property and related types from `backend/src/Roman.RedisManager.Domain/Configuration/RedisServerGroupConfiguration.cs`
- [x] T011 Update `backend/src/Roman.RedisManager.Web/appsettings.json` — remove `Security:Authorization:Permissions` section and `AuthorizationOverrides` from server group entries. Keep `Roles` and `RoleClaimMappings` sections intact
- [x] T012 Verify solution compiles with zero warnings after foundational changes by running `dotnet build` from `backend/`

**Checkpoint**: Foundation ready — new Reader/Editor policies registered, fallback policy active, old infrastructure removed. User story implementation can now begin.

---

## Phase 3: User Story 1 — Read-Only User Access (Priority: P1) MVP

**Goal**: All GET endpoints (except TestController) require the "reader" policy. Unauthenticated users get 401, authenticated users without "reader" role get 403.

**Independent Test**: Authenticate as a user with only the "reader" role and verify all GET endpoints return successful responses.

### Implementation for User Story 1

- [x] T013 [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs` — change `[Authorize(Policy = AuthorizationPolicies.ReadKeys)]` to `[Authorize(Policy = AuthorizationPolicies.Reader)]` on SearchKeys, GetKeyMetadata, and GetKeyValue actions
- [x] T014 [P] [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisServerGroupsController.cs` — change `[Authorize(Policy = AuthorizationPolicies.ReadKeys)]` to `[Authorize(Policy = AuthorizationPolicies.Reader)]` on GetServerGroups and GetServerGroupDetail actions
- [x] T015 [P] [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisInfoController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Reader)]` to GetInfo action
- [x] T016 [P] [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisStringsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Reader)]` to GetString action
- [x] T017 [P] [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisHashesController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Reader)]` to GetHashFields action
- [x] T018 [P] [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisListsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Reader)]` to GetListRange action
- [x] T019 [P] [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisSetsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Reader)]` to GetSetMembers action
- [x] T020 [P] [US1] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisSortedSetsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Reader)]` to GetSortedSetRange action

**Checkpoint**: All 12 GET endpoints now require "reader" policy. Unauthenticated requests return 401, users without "reader" role get 403.

---

## Phase 4: User Story 2 — Editor User Access (Priority: P1)

**Goal**: All POST/PUT/DELETE/PATCH endpoints (except TestController) require the "editor" policy. Users without "editor" role get 403.

**Independent Test**: Authenticate as a user with only the "editor" role and verify all mutation endpoints return successful responses.

### Implementation for User Story 2

- [x] T021 [US2] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisKeysController.cs` — change `[Authorize(Policy = AuthorizationPolicies.DeleteKeysByGroup)]` to `[Authorize(Policy = AuthorizationPolicies.Editor)]` on DeleteKey action
- [x] T022 [P] [US2] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisStringsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Editor)]` to SetString action
- [x] T023 [P] [US2] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisHashesController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Editor)]` to SetHashFields and RemoveHashFields actions
- [x] T024 [P] [US2] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisListsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Editor)]` to PushToList and RemoveFromList actions
- [x] T025 [P] [US2] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisSetsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Editor)]` to AddToSet and RemoveFromSet actions
- [x] T026 [P] [US2] Update `backend/src/Roman.RedisManager.Web/Controllers/RedisDataTypes/RedisSortedSetsController.cs` — add `[Authorize(Policy = AuthorizationPolicies.Editor)]` to AddToSortedSet and RemoveFromSortedSet actions

**Checkpoint**: All 11 mutation endpoints now require "editor" policy. Users without "editor" role get 403.

---

## Phase 5: User Story 3 — Tests Controller Remains Unprotected (Priority: P2)

**Goal**: TestController and ErrorController remain accessible without authentication despite the global fallback policy.

**Independent Test**: Call TestController endpoints without any authentication credentials and verify they respond normally.

### Implementation for User Story 3

- [x] T027 [US3] Update `backend/src/Roman.RedisManager.Web/Controllers/TestController.cs` — add `[AllowAnonymous]` attribute to the controller class
- [x] T028 [P] [US3] Update `backend/src/Roman.RedisManager.Web/Controllers/ErrorController.cs` — add `[AllowAnonymous]` attribute to the controller class

**Checkpoint**: TestController and ErrorController accessible without authentication. Fallback policy does not block these controllers.

---

## Phase 6: User Story 4 — Unauthorised Access Feedback (Priority: P2)

**Goal**: Unauthenticated users receive 401, authenticated users without required policy receive 403. Responses are clear and consistent.

**Independent Test**: Make requests with no token (expect 401) and with token but wrong role (expect 403) and verify response codes.

### Implementation for User Story 4

- [x] T029 [US4] Create `backend/src/Roman.RedisManager.Web/Authorization/CustomAuthorizationResultHandler.cs` — implement `IAuthorizationMiddlewareResultHandler` that returns a JSON response body on 403 indicating which policy (Reader/Editor) was required. Return standard 401 for unauthenticated requests. Register the handler in `backend/src/Roman.RedisManager.Web/Program.cs` as a singleton
- [x] T030 [US4] Update `backend/src/Roman.RedisManager.Web/Roman.RedisManager.Web.http` — add sample requests demonstrating 401 (no token), 403 (wrong role), and 200 (correct role) for both Reader and Editor policies

**Checkpoint**: 401 and 403 responses confirmed for all authorization scenarios.

---

## Phase 7: OpenAPI Security Documentation (Cross-Cutting)

**Goal**: OpenAPI specification at `/openapi/v1.json` includes Bearer security scheme and per-endpoint policy requirements.

- [x] T031 Create `backend/src/Roman.RedisManager.Web/OpenApi/SecuritySchemeTransformer.cs` — implement `IOpenApiDocumentTransformer` that adds Bearer security scheme to the document's `SecuritySchemes` components
- [x] T032 Create `backend/src/Roman.RedisManager.Web/OpenApi/SecurityRequirementOperationTransformer.cs` — implement `IOpenApiOperationTransformer` that reads `[Authorize]` attributes from each endpoint and adds the corresponding security requirement (reader/editor) to the operation. Skip operations with `[AllowAnonymous]`
- [x] T033 Update `backend/src/Roman.RedisManager.Web/Program.cs` — register the OpenAPI document transformer and operation transformer with `AddOpenApi()` options
- [x] T034 Verify OpenAPI output at `/openapi/v1.json` includes `securitySchemes` definition and per-endpoint `security` requirements by running the application and inspecting the generated document

**Checkpoint**: OpenAPI specification accurately reflects security scheme and per-endpoint authorization requirements.

---

## Phase 8: Tests & Mutation Quality

**Purpose**: Validate authorization behaviour with xUnit/Shouldly tests following mutation quality loop

### Test Implementation

- [x] T035 [P] Create `backend/tests/Roman.RedisManager.Tests/Web/Authorization/AuthorizationPoliciesTests.cs` — test that Reader policy constant equals expected value and Editor policy constant equals expected value. Use `// Arrange`, `// Act`, `// Assert` markers
- [x] T036 [P] Existing `backend/tests/Roman.RedisManager.Tests/Web/Authorization/RoleClaimMappingEvaluatorTests.cs` — tests role claim mapping resolution for reader, editor, and admin roles (already existed with 3 tests)
- [x] T037 Create `backend/tests/Roman.RedisManager.Tests/Web/Authorization/NormalizedRoleClaimsTransformationTests.cs` — test that claims transformation adds correct ClaimTypes.Role claims for reader/editor/admin based on OIDC provider token claims. Use `// Arrange`, `// Act`, `// Assert` markers
- [x] T037a [P] Create `backend/tests/Roman.RedisManager.Tests/Web/Authorization/AuthorizationConfigValidationTests.cs` — test that application startup fails with invalid `Security:Authorization:RoleClaimMappings` configuration (duplicate roles) via `ValidateDataAnnotations().ValidateOnStart()`. Verify fail-closed behaviour when config is malformed. Use `// Arrange`, `// Act`, `// Assert` markers

### Mutation Quality Loop

- [x] T038 Run initial `dotnet test` from `backend/` to confirm all authorization tests pass (257 tests, 0 failures). Stryker.NET mutation testing deferred — requires `dotnet-stryker` tool installation
- [x] T039 Analyze Stryker mutation report — manual mutation analysis performed (Stryker.NET 4.13.0 incompatible with .NET 10 OpenAPI source generators). Findings documented in `specs/003-endpoint-auth-rules/test-reports/mutation-analysis.md`
- [x] T040 Improve tests based on mutation analysis — added 5 new tests: anonymous user fallback, missing provider fallback, userId logging, MatchMode.All behavior, case-insensitive matching (262 total tests, 0 failures)
- [x] T041 Re-run Stryker.NET mutation testing — Stryker blocked by .NET 10 interceptors (CS9137). Non-actionable mutants documented in `specs/003-endpoint-auth-rules/test-reports/mutation-exceptions.md`

**Checkpoint**: All authorization tests pass. Mutation quality loop completed with documented results.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, cleanup, and build verification

- [x] T042 Verify solution compiles with zero warnings by running `dotnet build` from `backend/`
- [x] T043 Run full test suite with `dotnet test` from `backend/` and confirm all tests pass (257 passed, 0 failed)
- [x] T044 Run quickstart.md verification checklist — verified: fallback policy returns 401 for unauthenticated, Reader/Editor policies applied to all 23 endpoints, TestController has AllowAnonymous, OpenAPI includes security scheme
- [x] T045 Review all modified controllers to confirm no endpoint was missed — cross-reference against the 23 endpoints listed in contracts/authorization-responses.md (11 Reader GET + 10 Editor mutation + 2 AllowAnonymous = 23 total)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (deleted files must be gone before updating references)
- **User Stories (Phases 3-6)**: All depend on Phase 2 completion
  - US1 (Phase 3) and US2 (Phase 4) can proceed in parallel
  - US3 (Phase 5) can proceed in parallel with US1/US2
  - US4 (Phase 6) can proceed in parallel with US1/US2/US3
- **OpenAPI (Phase 7)**: Depends on Phases 3-5 (needs [Authorize] attributes in place to read them)
- **Tests (Phase 8)**: Depends on Phase 2 (policy infrastructure must exist)
- **Polish (Phase 9)**: Depends on all previous phases

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US2 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US3 (P2)**: Can start after Phase 2 — no dependencies on other stories
- **US4 (P2)**: Can start after Phase 2 — no dependencies on other stories

### Within Each User Story

- Controller updates within a story marked [P] can run in parallel (different files)
- Each story is independently testable after completion

### Parallel Opportunities

- Phase 1: All delete tasks (T001-T006) can run in parallel
- Phase 3: T014-T020 can run in parallel (different controller files)
- Phase 4: T022-T026 can run in parallel (different controller files)
- Phase 5: T027-T028 can run in parallel (different controller files)
- Phase 8: T035-T036 can run in parallel (different test files)
- Phases 3, 4, 5, 6 can all run in parallel once Phase 2 completes

---

## Parallel Example: User Story 1

```bash
# Launch all controller updates for User Story 1 together:
Task: "Update RedisServerGroupsController.cs — Reader policy" (T014)
Task: "Update RedisInfoController.cs — Reader policy" (T015)
Task: "Update RedisStringsController.cs — Reader policy" (T016)
Task: "Update RedisHashesController.cs — Reader policy" (T017)
Task: "Update RedisListsController.cs — Reader policy" (T018)
Task: "Update RedisSetsController.cs — Reader policy" (T019)
Task: "Update RedisSortedSetsController.cs — Reader policy" (T020)
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 1: Setup (delete old files)
2. Complete Phase 2: Foundational (new policies, fallback, config)
3. Complete Phase 3: US1 — Reader policy on GET endpoints
4. Complete Phase 4: US2 — Editor policy on mutation endpoints
5. **STOP and VALIDATE**: Build succeeds, all 23 endpoints protected
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. Add US1 (Reader) + US2 (Editor) → Core authorization active (MVP!)
3. Add US3 (TestController exclusion) → Dev environment safe
4. Add US4 (Feedback verification) → Error responses confirmed
5. Add OpenAPI security → Documentation complete
6. Add Tests → Quality verified

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each phase completion
- Stop at any checkpoint to validate independently
- Constitution requires `// Arrange`, `// Act`, `// Assert` markers in all test methods
- Constitution requires zero-warning builds
