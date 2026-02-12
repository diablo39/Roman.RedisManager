## Plan: Make RedisRepository group-aware (no factory)

We’ll drop the factory idea and make repository methods accept `groupId`, updating call sites and registrations accordingly while keeping Clean Architecture and Wolverine patterns.

**Steps**
1. Update contract in [src/Roman.RedisManager.Domain/Repositories/IRedisRepository.cs](src/Roman.RedisManager.Domain/Repositories/IRedisRepository.cs) so both `SearchForKeys` and `GetServerNodesAsync` accept `groupId`; adjust XML docs/comments if present.
2. Refactor implementation in [src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs](src/Roman.RedisManager.Infrastructure/Repositories/RedisRepository.cs):
   - Add `groupId` parameter to `SearchForKeys`.
   - Remove default-group lookup; resolve the group via `groupId` (reuse existing ResolveServerGroup logic).
   - Ensure `SearchForKeys` uses async connection flow (avoid synchronous blocking) and reuses connection manager per `groupId`.
   - Keep thread safety (no static/shared mutable state changes).
3. Adjust DI in [src/Roman.RedisManager.Web/Program.cs](src/Roman.RedisManager.Web/Program.cs):
   - Keep `IRedisRepository` registration (likely singleton acceptable if stateless) or switch to scoped/transient if desired; ensure it aligns with new signature (no factory).
4. Update Wolverine handlers that call the repository to pass `groupId` explicitly:
   - [src/Roman.RedisManager.Application/CQRS/RedisServerGroupDetailQueryHandler.cs](src/Roman.RedisManager.Application/CQRS/RedisServerGroupDetailQueryHandler.cs) and any others using `IRedisRepository`.
5. Update any controller invocations if they directly use `IRedisRepository` (likely none; handlers use Wolverine).
6. Fix tests:
   - [tests/Roman.RedisManager.Tests/RedisRepositoryTests.cs](tests/Roman.RedisManager.Tests/RedisRepositoryTests.cs) and [tests/Roman.RedisManager.Tests/RedisServerGroupDetailQueryHandlerTests.cs](tests/Roman.RedisManager.Tests/RedisServerGroupDetailQueryHandlerTests.cs) to call the new method signatures with `groupId`.
7. Do a quick search for `SearchForKeys(` and `GetServerNodesAsync(` usages to ensure all call sites pass `groupId`.

**Verification**
- Run unit tests in tests/Roman.RedisManager.Tests after signature updates.
- Build solution to confirm DI and handler wiring compile with new signatures.

**Decisions**
- No factory; repository stays but becomes group-aware via method parameters.
- Caller responsibility: always supply `groupId` for repository operations.
