# Tech Stack Analysis

## Core Technology Analysis

### Programming Languages
- **C# (.NET 10.0)**: Backend API and web server
- **TypeScript (5.9.2)**: Frontend application logic
- **HTML/CSS**: UI markup and styling

### Primary Framework
- **ASP.NET Core 10.0 (Web API)**: Backend framework serving as the API layer and static file host
- **Angular 20.3.0**: Frontend SPA framework using standalone components (no modules)

### Secondary/Tertiary Frameworks
- **PrimeNG 20.3.0**: UI component library
  - PrimeIcons 7.0.0: Icon library
  - PrimeFlex 3.3.1: CSS utility framework
  - @primeuix/themes 2.0.1: Theming system (using Aura preset)
- **RxJS 7.8.0**: Reactive programming for Angular
- **Jasmine/Karma**: Testing framework for frontend

### State Management Approach
- **Angular Signals**: Modern reactive state management (evidenced by `signal()` usage in App component)
- **No centralized state management library**: No Redux, NgRx, or similar libraries detected
- Component-level state using signals and Angular's built-in reactivity

### Other Relevant Technologies & Patterns
- **OpenAPI/Swagger**: API documentation (Microsoft.AspNetCore.OpenApi 10.0.0)
- **SPA Architecture**: ASP.NET Core serves Angular built files and provides fallback routing
- **Standalone Components**: Angular application uses the modern standalone component API (no NgModules)
- **Zone.js**: Angular change detection mechanism
- **Build Integration**: .NET build process automatically builds and bundles Angular app into wwwroot
- **TypeScript Strict Mode**: Enabled with comprehensive strictness flags (strict, noImplicitOverride, noImplicitReturns, noFallthroughCasesInSwitch)
- **Prettier**: Code formatting with specific Angular HTML parser configuration

## Domain Specificity Analysis

### Problem Domain
This application targets **Redis database management and administration**. It's a web-based tool for managing Redis instances, connections, and data operations.

### Core Business Concepts
- **Redis Connection Management**: Managing connections to Redis servers/clusters
- **Database Administration**: CRUD operations on Redis data structures (strings, hashes, lists, sets, sorted sets, etc.)
- **Monitoring and Analytics**: Potentially tracking Redis server health, performance metrics, and usage statistics
- **Data Visualization**: Displaying Redis data in a user-friendly interface

### User Interactions
- **Connection Configuration**: Users configure and manage Redis server connections
- **Data Browsing**: Navigate through Redis keys, databases, and data structures
- **Data Manipulation**: Create, read, update, and delete Redis entries
- **Query Execution**: Run Redis commands and view results
- **Dashboard Views**: Monitor Redis instance status and metrics

### Primary Data Types and Structures
- **Connection Configurations**: Server URLs, ports, authentication credentials
- **Redis Data Structures**: Keys, values, hashes, lists, sets, sorted sets, streams
- **Metadata**: Database indexes, key expiration times, data types
- **API DTOs**: Data transfer objects for client-server communication (currently minimal with WeatherForecast example)

## Application Boundaries

### Features Within Scope (Based on Existing Code)
- **SPA Frontend**: Single-page application with client-side routing
- **RESTful API Backend**: JSON-based API endpoints for data operations
- **Static File Serving**: Serving Angular build artifacts
- **Development/Production Configurations**: Environment-specific settings
- **UI Component System**: Using PrimeNG component library for consistent UI
- **Theming Support**: Customizable themes via PrimeNG theme system
- **API Documentation**: OpenAPI/Swagger integration for API exploration

### Features Architecturally Consistent
- **Additional Redis Operations**: New controllers for keys, values, server commands
- **Real-time Updates**: WebSocket or SignalR integration for live data updates
- **Multi-database Support**: Managing multiple Redis databases within one instance
- **User Authentication/Authorization**: JWT or cookie-based auth for securing connections
- **Redis Command Terminal**: Interactive command-line interface within the UI
- **Import/Export Functionality**: Backup and restore Redis data
- **Search and Filtering**: Advanced key search and data filtering capabilities
- **Redis Pub/Sub Monitoring**: Subscribe to and monitor Redis channels
- **Configuration Management**: Save and load connection profiles

### Features Outside Scope/Architecturally Inconsistent
- **Desktop Application**: This is explicitly a web-based solution
- **Direct Database Connections from Frontend**: All Redis operations must go through the backend API
- **Non-Redis Database Support**: Architecture is Redis-specific
- **Heavy Data Processing**: Large-scale data transformations should occur on the backend
- **File System Operations**: Direct file access from the frontend
- **Embedded Redis Server**: Application is a manager, not a Redis host

### Specialized Libraries/Constraints
- **PrimeNG Ecosystem**: UI development should leverage PrimeNG components for consistency
- **Angular Standalone Architecture**: New components should use standalone component pattern
- **Signal-based Reactivity**: State management should prefer signals over traditional observables where appropriate
- **.NET Minimal API or Controller Pattern**: Backend follows ASP.NET Core Web API conventions
- **SPA Routing Fallback**: All non-API routes fall back to index.html for Angular routing
