# Domain: Testing

## Overview
The testing infrastructure uses **Jasmine** as the testing framework and **Karma** as the test runner. Tests are colocated with source files using the `.spec.ts` extension.

## Testing Framework

### Jasmine and Karma
The project uses Jasmine for writing tests and Karma for executing them.

**From `package.json`:**
```json
{
  "devDependencies": {
    "@types/jasmine": "~5.1.0",
    "jasmine-core": "~5.9.0",
    "karma": "~6.4.0",
    "karma-chrome-launcher": "~3.2.0",
    "karma-coverage": "~2.2.0",
    "karma-jasmine": "~5.1.0",
    "karma-jasmine-html-reporter": "~2.1.0"
  }
}
```

**Test Execution Command:**
```json
{
  "scripts": {
    "test": "ng test"
  }
}
```

## Test File Structure

### File Naming Convention
Test files use the `.spec.ts` extension and are colocated with the source files.

**Example:**
- Source: `src/app/app.ts`
- Test: `src/app/app.spec.ts`

### Component Test Example
**From `src/app/app.spec.ts`:**
```typescript
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have the 'roman-redis-manager-ui' title`, () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app.title()).toEqual('roman-redis-manager-ui');
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Hello, roman-redis-manager-ui');
  });
});
```

## Testing Patterns

### TestBed Configuration
Tests use `TestBed` to configure the testing module.

**Pattern for Standalone Components:**
```typescript
beforeEach(async () => {
  await TestBed.configureTestingModule({
    imports: [App],  // Import standalone component
  }).compileComponents();
});
```

**Key Points:**
- **imports array**: Standalone components are imported, not declared
- **async/await**: Component compilation is asynchronous
- **compileComponents()**: Compiles templates and styles

### Component Creation
Components are created using `TestBed.createComponent()`.

```typescript
const fixture = TestBed.createComponent(App);
const app = fixture.componentInstance;
```

**Patterns:**
- **ComponentFixture**: Wrapper for component testing
- **componentInstance**: Access to component class instance
- **nativeElement**: Access to rendered DOM

### Testing Signals
Signals are tested by calling them as functions.

```typescript
it(`should have the 'roman-redis-manager-ui' title`, () => {
  const fixture = TestBed.createComponent(App);
  const app = fixture.componentInstance;
  expect(app.title()).toEqual('roman-redis-manager-ui');  // Call signal as function
});
```

**Pattern:**
- Signals accessed with function call: `app.title()`
- Test both signal value and reactivity

### DOM Testing
Tests can query the rendered DOM.

```typescript
it('should render title', () => {
  const fixture = TestBed.createComponent(App);
  fixture.detectChanges();  // Trigger change detection
  const compiled = fixture.nativeElement as HTMLElement;
  expect(compiled.querySelector('h1')?.textContent).toContain('Hello, roman-redis-manager-ui');
});
```

**Patterns:**
- **detectChanges()**: Manually trigger change detection
- **nativeElement**: Access to actual DOM
- **querySelector**: Standard DOM querying
- **Type casting**: Cast to HTMLElement for TypeScript

## Test Configuration

### TypeScript Configuration
Test-specific TypeScript configuration in `tsconfig.spec.json`.

**From `tsconfig.spec.json`:**
```json
{
  "compilerOptions": {
    "strict": true,
    // ... other strict options
  },
  "files": [
    "src/polyfills.spec.ts"
  ],
  "include": [
    "src/**/*.spec.ts",
    "src/**/*.d.ts"
  ]
}
```

### Angular Test Configuration
**From `angular.json`:**
```json
{
  "test": {
    "builder": "@angular/build:karma",
    "options": {
      "polyfills": [
        "zone.js",
        "zone.js/testing"
      ],
      "tsConfig": "tsconfig.spec.json",
      "assets": [
        {
          "glob": "**/*",
          "input": "public"
        }
      ],
      "styles": [
        "src/styles.css"
      ]
    }
  }
}
```

**Configuration Elements:**
- **Polyfills**: Zone.js for change detection and testing
- **tsConfig**: Points to test-specific TypeScript config
- **assets**: Include public assets in tests
- **styles**: Include global styles

## Test Organization

### Describe Blocks
Tests are organized using `describe()` blocks.

```typescript
describe('App', () => {
  // Tests for App component
});
```

**Patterns:**
- Top-level describe named after component/class
- Nested describes for logical groupings

### BeforeEach Hooks
Setup code runs before each test.

```typescript
beforeEach(async () => {
  await TestBed.configureTestingModule({
    imports: [App],
  }).compileComponents();
});
```

**Common Uses:**
- TestBed configuration
- Mock setup
- Fixture creation

### Test Cases
Individual tests use `it()` blocks.

```typescript
it('should create the app', () => {
  const fixture = TestBed.createComponent(App);
  const app = fixture.componentInstance;
  expect(app).toBeTruthy();
});
```

**Patterns:**
- Descriptive test names
- Single assertion focus (when possible)
- Arrange-Act-Assert pattern

## Testing Best Practices

### Component Testing
1. **Test creation**: Verify component instantiates
2. **Test properties**: Verify signals and properties
3. **Test rendering**: Verify DOM output
4. **Test interactions**: Verify user actions
5. **Test integration**: Verify child components

### Standalone Component Testing
1. **Import in TestBed**: Use `imports` array, not `declarations`
2. **Import dependencies**: Include all component dependencies
3. **Mock services**: Provide mock services when needed

### Signal Testing
1. **Call as functions**: Access signal values with `()`
2. **Test reactivity**: Verify signal updates propagate
3. **Test computed signals**: Verify derived values

## Summary

The testing domain follows these principles:

1. **Jasmine + Karma**: Standard Angular testing stack
2. **Colocated tests**: `.spec.ts` files next to source files
3. **TestBed configuration**: For standalone components
4. **Signal-aware testing**: Call signals as functions
5. **DOM testing**: Use nativeElement and detectChanges
6. **Strict TypeScript**: Same strict settings as production
