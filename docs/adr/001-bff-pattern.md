# ADR-001: Backend for Frontend (BFF) Pattern

## Status
Proposed

## Context
Modern web applications often have multiple client interfaces (web, mobile, third-party integrations) that have different data requirements, API response formats, and performance characteristics. A monolithic backend API typically serves all clients with a single interface, leading to several challenges:
- Over-fetching/under-fetching of data
- Complex client-side logic for data transformation
- Tight coupling between client and server
- Difficulty optimizing for specific client needs

## Decision
Adopt the Backend for Frontend (BFF) pattern, where dedicated intermediate layers are created for each client type (Web BFF, Mobile BFF, etc.) that sit between client applications and the core backend services.

## Architecture Overview

```
┌─────────────┐     ┌─────────────┐     ┌──────────────────────────┐
│   Web App   │────▶│   Web BFF   │────▶│                          │
└─────────────┘     └─────────────┘     │                          │
                                          │   Core Backend           │
┌─────────────┐     ┌─────────────┐     │   Services               │
│ Mobile App  │────▶│ Mobile BFF  │────▶│   (Domain, Repositories) │
└─────────────┘     └─────────────┘     │                          │
                                          │   Data Layer             │
┌─────────────┐     ┌─────────────┐     │   (Redis, DB, etc.)      │
│ 3rd Party   │────▶│  API BFF    │────▶│                          │
└─────────────┘     └─────────────┘     └──────────────────────────┘
```

## Reasons to Use BFF Pattern

### 1. Tailored Data Shape per Client
Each client type has unique data requirements:
- **Web clients**: Can handle larger payloads, benefit from reduced round-trips
- **Mobile clients**: Need smaller payloads, optimized for bandwidth
- **Third-party APIs**: Require specific, stable contracts

The BFF transforms data to exactly what each client needs, eliminating over-fetching.

### 2. Reduced Client-Side Complexity
Without BFF, clients must perform significant data transformation:
- Merging multiple API responses
- Filtering and mapping data structures
- Implementing business logic that belongs server-side

With BFF, clients receive ready-to-use data, keeping client code simple and focused on presentation.

### 3. Performance Optimization
- **Network optimization**: BFFs can batch multiple downstream calls into single round-trips for clients
- **Caching**: Client-specific caching strategies can be implemented per BFF
- **Payload size**: Mobile BFFs can strip unnecessary fields, reducing data transfer

### 4. Independent Deployment
Each BFF can be deployed independently:
- Web BFF changes don't impact mobile clients
- Feature flags can be implemented at BFF level
- A/B testing scenarios can isolate changes to specific client types

### 5. Separation of Concerns
- **Core backend**: Focuses on domain logic, business rules, and data integrity
- **BFFs**: Handle presentation concerns, client-specific orchestrations, and API contracts
- Clean boundaries enable teams to work independently

### 6. Client Lifecycle Management
Different clients evolve at different rates:
- Mobile apps have app store review cycles
- Web apps can update instantly
- Third-party integrations require stable contracts

BFFs provide a buffer that shields the core backend from these varying constraints.

### 7. Security and Authorization
- Client-specific authentication flows can be implemented per BFF
- Fine-grained authorization rules can be enforced per client type
- API keys and rate limiting can be managed per client

### 8. Testing and Development
- Each BFF can be mocked independently for frontend development
- End-to-end tests can be scoped to specific client flows
- Reduced cognitive load for developers (smaller, focused codebases)

### 9. Technology Flexibility
Different BFFs can use different technologies:
- Web BFF: ASP.NET Core with server-side rendering support
- Mobile BFF: Lightweight, optimized for JSON responses
- API BFF: GraphQL gateway or REST proxy

### 10. Migration Path
BFFs provide a natural boundary for gradual migrations:
- Legacy APIs can be wrapped in a BFF while core services are refactored
- New clients can be added without modifying existing infrastructure

## Implementation Considerations

### Potential Drawbacks
| Concern | Mitigation |
|---------|------------|
| Code duplication across BFFs | Shared libraries for common orchestrations |
| Increased infrastructure | Containerization and orchestration reduce overhead |
| Version coordination | Semantic versioning and contract testing |
| Debugging complexity | Centralized logging and tracing |

### When to Use BFF
- Multiple distinct client types with different needs
- High-traffic applications where optimization matters
- Teams organized by client platform (Web team, Mobile team)
- Need for independent deployment cycles

### When NOT to Use BFF
- Single client application
- Small team maintaining one product
- Simple CRUD applications without complex client needs

## Consequences
- **Positive**: Better client experience, reduced complexity, improved performance
- **Positive**: Independent deployments, team autonomy
- **Negative**: Additional infrastructure to manage
- **Negative**: Requires upfront design consideration

## References
- [Pattern: Backend for Frontend - Microsoft](https://learn.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends)
- [The BFF Pattern - Sam Newman](https://samnewman.io/patterns/architectural/bff/)
- [Backend for Frontend - ThoughtWorks](https://www.thoughtworks.com/radar/techniques/backends-for-frontends)