# Testing Domain Analysis

## Overview
The testing domain uses Jasmine as the testing framework with Karma as the test runner, configured for Angular's standalone component architecture.

## Key Patterns and Conventions

### Test File Naming
Test files follow a consistent naming convention:

**Pattern:**
- Component tests: `{name}.spec.ts`
- Location: Colocated with the component being tested

**Example:**
```
src/app/app.ts
src/app/app.spec.ts
```

### TestBed Configuration for Standalone Components
Tests use Angular's TestBed with standalone component imports:

**From `src/app/app.spec.ts`:**
```typescript
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],  // Import standalone component directly
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Hello, roman-redis-manager-ui');
  });
});
```

**Key Patterns:**

1. **Async beforeEach:**
   ```typescript
   beforeEach(async () => {
     await TestBed.configureTestingModule({
       imports: [App],
     }).compileComponents();
   });
   ```
   - `beforeEach` marked as `async`
   - `await` on `compileComponents()` for async template compilation
   - Standalone component added to `imports` array (not `declarations`)

2. **Component Creation:**
   ```typescript
   const fixture = TestBed.createComponent(App);
   const app = fixture.componentInstance;
   ```
   - Create component fixture via `TestBed.createComponent()`
   - Access component instance via `fixture.componentInstance`

3. **Change Detection:**
   ```typescript
   fixture.detectChanges();
   ```
   - Manually trigger change detection before assertions
   - Required to update the DOM with component data

4. **DOM Queries:**
   ```typescript
   const compiled = fixture.nativeElement as HTMLElement;
   expect(compiled.querySelector('h1')?.textContent).toContain('Hello, roman-redis-manager-ui');
   ```
   - Access DOM via `fixture.nativeElement`
   - Type assertion to `HTMLElement` for type safety
   - Use standard DOM query methods (`querySelector`, etc.)
   - Optional chaining (`?.`) for safe property access

### Test Structure
Tests follow Jasmine's BDD-style syntax:

**Pattern:**
```typescript
describe('ComponentName', () => {
  beforeEach(async () => {
    // Setup
  });

  it('should do something', () => {
    // Arrange
    // Act
    // Assert
  });
});
```

**Conventions:**
- `describe()`: Groups related tests
- `it()`: Individual test case with descriptive name
- `beforeEach()`: Setup code run before each test
- `expect()`: Assertions using Jasmine matchers

### TypeScript Configuration for Tests
**From `tsconfig.spec.json`:**
```json
{
  "extends": "./tsconfig.json",
  "compilerOptions": {
    "outDir": "./out-tsc/spec",
    "types": ["jasmine"]
  },
  "include": [
    "**/*.spec.ts"
  ]
}
```

**Key Settings:**
- Extends base `tsconfig.json`
- Includes Jasmine types for test syntax
- Only includes `.spec.ts` files

## Tools and Technologies

### Testing Framework
- **Jasmine 5.9.0**: BDD-style testing framework
- **Karma 6.4.0**: Test runner for browsers
- **@angular/core/testing**: Angular testing utilities
- **TestBed**: Angular's primary testing API

### Test Runners and Reporters
- **karma-jasmine 5.1.0**: Jasmine adapter for Karma
- **karma-jasmine-html-reporter 2.1.0**: HTML test results
- **karma-chrome-launcher 3.2.0**: Chrome browser launcher
- **karma-coverage 2.2.0**: Code coverage reporting

### Build Integration
- Tests run via `ng test` command
- Configuration in `angular.json` test architect

## Implementation Guidelines

### Writing Component Tests
Standard component test structure:

```typescript
import { TestBed } from '@angular/core/testing';
import { MyComponent } from './my-component';

describe('MyComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent],  // Standalone component
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(MyComponent);
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });

  it('should display initial value', () => {
    const fixture = TestBed.createComponent(MyComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.value')?.textContent).toBe('Initial');
  });
});
```

### Testing Components with Dependencies
When component has dependencies (services, other components):

```typescript
import { TestBed } from '@angular/core/testing';
import { MyComponent } from './my-component';
import { MyService } from './my-service';
import { DependencyComponent } from './dependency-component';

describe('MyComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent, DependencyComponent],  // Import all standalone components
      providers: [MyService]  // Provide services
    }).compileComponents();
  });

  // ... tests
});
```

### Testing with Mocks
For service mocking:

```typescript
const mockService = jasmine.createSpyObj('MyService', ['getData']);

await TestBed.configureTestingModule({
  imports: [MyComponent],
  providers: [
    { provide: MyService, useValue: mockService }
  ]
}).compileComponents();
```

### Testing Signal-Based Components
For components using signals:

```typescript
it('should update signal value', () => {
  const fixture = TestBed.createComponent(MyComponent);
  const component = fixture.componentInstance;
  
  component.mySignal.set('new value');
  fixture.detectChanges();
  
  expect(component.mySignal()).toBe('new value');
});
```

### Testing Router Integration
For components with routing:

```typescript
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

beforeEach(async () => {
  await TestBed.configureTestingModule({
    imports: [MyComponent],
    providers: [provideRouter(routes)]
  }).compileComponents();
});
```

### Running Tests
**Commands:**
```bash
ng test                    # Run tests in watch mode
ng test --watch=false     # Run tests once
ng test --code-coverage   # Generate coverage report
```

**Configuration:**
- Tests run in Chrome (headless available)
- Watch mode enabled by default
- Coverage reports in `coverage/` directory

### Best Practices
1. **Descriptive test names**: Use "should" statements
2. **Arrange-Act-Assert**: Clear test structure
3. **Async operations**: Always `await` async setup
4. **Change detection**: Call `fixture.detectChanges()` before DOM assertions
5. **Type safety**: Use TypeScript's type system in tests
6. **Mock dependencies**: Isolate component under test
7. **Test signals**: Test both reading and updating signal values
8. **Colocate tests**: Keep `.spec.ts` files next to source files
