<!--
  Sync Impact Report
  ==================
  Version change: 0.0.0 → 1.0.0 (MAJOR: initial ratification)
  Modified principles: N/A (initial version)
  Added sections:
    - Core Principles (5 principles)
    - Technology Stack & Constraints
    - Development Workflow & Quality Gates
    - Governance
  Removed sections: N/A
  Templates requiring updates:
    - .specify/templates/plan-template.md           ✅ no changes needed
    - .specify/templates/spec-template.md            ✅ no changes needed
    - .specify/templates/tasks-template.md           ✅ no changes needed
    - .specify/templates/checklist-template.md       ✅ no changes needed
    - .specify/templates/agent-file-template.md      ✅ no changes needed
  Follow-up TODOs: none
-->

# Roman.RedisManager Constitution

## Core Principles

### I. Clean Architecture

All code MUST follow the four-layer dependency direction:
**Domain → Application → Infrastructure → Web**.

- Domain MUST NOT reference Infrastructure, Application, Web,
  or any external NuGet package.
- Application MUST NOT reference Infrastructure or Web.
- Web depends on Application and Infrastructure for DI wiring.
- Infrastructure depends on Domain only.
- Every new feature MUST place files in the correct layer
  according to the Feature Scaffold Guide in
  `copilot-instructions.md`.

**Rationale:** Strict layering prevents coupling leaks and keeps
the domain model portable and testable in isolation.

### II. CQRS via Wolverine (NON-NEGOTIABLE)

All application logic MUST be dispatched through Wolverine's
`IMessageBus.InvokeAsync<T>()`.

- Controllers MUST NOT call repositories or services directly.
- Handlers MUST be **static classes** with **static `Handle` or
  `HandleAsync` methods**; no `[Handler]` attributes.
- The first parameter is the message (query/command); additional
  parameters are DI-injected by Wolverine.
- All CQRS types (query, DTOs, result, handler) MUST be
  co-located in a single file under
  `Application/CQRS/{Feature}QueryHandler.cs` or
  `{Feature}CommandHandler.cs`.

**Rationale:** Uniform mediation through Wolverine keeps
controllers thin and enables assembly-scanned discovery without
boilerplate.

### III. Repository Pattern

- Interfaces MUST live in `Domain/Repositories/`.
- Implementations MUST live in `Infrastructure/Repositories/`.
- Repositories MUST return domain entities — never
  StackExchange.Redis types.
- Repositories MUST be registered as **singletons** in
  `Program.cs`.
- No EF Core, Dapper, or other ORM is permitted; this project
  uses StackExchange.Redis directly.
- Entity-to-DTO mapping uses LINQ `.Select()` inline — no
  AutoMapper or similar libraries.

**Rationale:** Singleton lifetime matches the long-lived
`ConnectionMultiplexer`; returning domain types keeps
infrastructure details out of upper layers.

### IV. Test Discipline

- Use **xUnit** `[Fact]` tests with **Shouldly** assertions.
- Method naming: `MethodName_Condition_ExpectedBehavior`.
- Instantiate **real implementations** — no Moq, NSubstitute, or
  other mocking frameworks.
- Use `Options.Create(...)` with private helper methods for
  configuration in tests.
- Declare repositories as interface types:
  `IRedisServerRepository repo = new RedisServerRepository(...)`.
- Global usings for `Xunit` and `Shouldly` in the test project.

**Rationale:** Real implementations exercise the full stack
against a live Redis instance, catching integration issues that
mocks would hide.

### V. Consistency & Simplicity

- Block-scoped namespaces everywhere (never file-scoped).
- `private readonly` fields with `_camelCase` prefix.
- Expression-bodied constructors for single-dependency classes.
- Configuration classes MUST use the Options pattern with
  `required` keyword + `[Required]` / `[MinLength]` attributes,
  `[ConfigurationKeyName]` on every property, and
  `ValidateDataAnnotations().ValidateOnStart()` at registration.
- Collection properties typed as `IEnumerable<T>`; return types
  as `IReadOnlyCollection<T>`.
- Controller routes use `[Route("api/{kebab-case-resource}")]`.
- Controllers return DTOs directly for normal endpoints; **exception**: dedicated error controllers or actions **may** return `ProblemDetails`/`IActionResult` when handling error routes invoked by middleware (e.g. `/error` used by `UseExceptionHandler`). This exception is limited to the error-handling path and does not permit arbitrary controller actions to return generic `IActionResult`.
- Only PowerShell scripts for automation tasks.

**Rationale:** A single, explicit style eliminates bike-shedding
and makes AI-generated code immediately consistent with
hand-written code.

## Technology Stack & Constraints

| Technology | Version | Purpose |
|---|---|---|
| .NET | 10.0 | Runtime & SDK |
| ASP.NET Core | 10.0 | Web API framework |
| Wolverine | 5.9.2 | In-process CQRS mediator |
| StackExchange.Redis | 2.10.1 | Redis client |
| xUnit | 2.9.3 | Test framework |
| Shouldly | 4.3.0 | Assertion library |
| Swashbuckle | 10.1.0 | Swagger UI (Dev only) |

Additional constraints:

- The Angular SPA frontend is served via ASP.NET static files
  with a fallback to `index.html`.
- Swagger/OpenAPI endpoints are enabled only in the Development
  environment.
- Docker Compose in `/docker` provides Redis master/slave and
  cluster topologies for local development.
- No additional NuGet packages may be introduced without
  explicit justification and constitution amendment.

## Development Workflow & Quality Gates

1. **Feature Scaffold** — follow the step-by-step table in
   `copilot-instructions.md` § "Feature Scaffold Guide" to
   determine which files to create and where.
2. **Domain First** — model entities and repository interfaces
   before touching infrastructure or web layers.
3. **Handler + Controller** — implement the CQRS handler, then
   the controller action that dispatches to it.
4. **DI Registration** — add any new singletons to `Program.cs`
   following the existing registration order: Options →
   Singletons → Controllers → OpenApi → Wolverine.
5. **Tests** — add xUnit/Shouldly facts exercising the new
   repository or handler against a real Redis instance.
6. **HTTP File** — append a sample request to
   `Roman.RedisManager.Web.http`.
7. **Build Gate** — the solution MUST compile with zero warnings
   before a feature is considered complete.
8. **Mutation Test Gate** — when tests are added or changed, the workflow MUST execute `run -> analyze -> improve -> rerun`.

Mutation Test Gate enforcement:

- Mutation reports MUST include HTML and JSON artifacts.
- Surviving/no-coverage mutants MUST be triaged into actionable findings.
- Actionable findings MUST trigger assertion updates followed by another mutation run.
- If work ends with remaining survivors, an approved non-actionable exception record is REQUIRED.

## Governance

This constitution is the authoritative source of architectural
and coding standards for Roman.RedisManager. It supersedes any
conflicting guidance found elsewhere in the repository.

- All code reviews MUST verify compliance with the five core
  principles above.
- Introducing a new external dependency or deviating from any
  principle requires an amendment to this document with:
  (a) written justification, (b) impact analysis, and
  (c) a version bump.
- Amendments follow semantic versioning:
  - **MAJOR**: Principle removed, redefined, or backward-
    incompatible governance change.
  - **MINOR**: New principle or section added; materially
    expanded guidance.
  - **PATCH**: Wording clarifications, typo fixes, non-semantic
    refinements.
- Runtime development guidance lives in
  `.github/copilot-instructions.md` and the instruction files
  under `.github/instructions/`.

**Version**: 1.0.0 | **Ratified**: 2026-03-02 | **Last Amended**: 2026-03-02
