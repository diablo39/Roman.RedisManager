# Style Guide: Unit Tests

## Unique Patterns in This Codebase

### 1. Test File Naming and Colocation
**Standard pattern:** Tests colocated with source using `.spec.ts` extension.

**Example:**
```
app/
  app.ts
  app.html
  app.css
  app.spec.ts  ← Test file here
```

**Convention:**
- **`.spec.ts` extension**: All test files end with `.spec.ts`
- **Same directory**: Test files next to the code they test
- **Matching base name**: `app.ts` → `app.spec.ts`

---

### 2. Standalone Component Testing
**Modern Angular pattern:** Configure TestBed with imports, not declarations.

**Example:**
```typescript
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],  // Import standalone component
    }).compileComponents();
  });
  
  // tests...
});
```

**Convention:**
- **imports array**: Use `imports`, not `declarations`
- **Direct component import**: Import the component class
- **No module wrapping**: Standalone components imported directly
- **Async compilation**: Use `async/await` with `compileComponents()`

---

### 3. Signal Testing Pattern
**Signal-specific:** Test signals by calling them as functions.

**Example:**
```typescript
it(`should have the 'roman-redis-manager-ui' title`, () => {
  const fixture = TestBed.createComponent(App);
  const app = fixture.componentInstance;
  expect(app.title()).toEqual('roman-redis-manager-ui');  // Call signal
});
```

**Convention:**
- **Function call syntax**: `signal()` not `signal`
- **Test signal values**: Verify current value
- **Test signal updates**: When testing setters, verify reactivity
- **Computed signals**: Test derivations are correct

---

### 4. Component Existence Test
**Standard first test:** Always test that component creates successfully.

**Example:**
```typescript
it('should create the app', () => {
  const fixture = TestBed.createComponent(App);
  const app = fixture.componentInstance;
  expect(app).toBeTruthy();
});
```

**Convention:**
- **First test always**: Component creation test comes first
- **Simple assertion**: Just check truthiness
- **Descriptive name**: Use "should create the {component-name}"

---

### 5. DOM Rendering Tests
**Standard pattern:** Test rendered output using nativeElement.

**Example:**
```typescript
it('should render title', () => {
  const fixture = TestBed.createComponent(App);
  fixture.detectChanges();  // Trigger rendering
  const compiled = fixture.nativeElement as HTMLElement;
  expect(compiled.querySelector('h1')?.textContent)
    .toContain('Hello, roman-redis-manager-ui');
});
```

**Convention:**
- **Call detectChanges()**: Manually trigger change detection
- **Use nativeElement**: Access actual DOM
- **Type cast**: Cast to `HTMLElement` for TypeScript
- **querySelector**: Use standard DOM querying
- **Optional chaining**: Use `?.` to avoid null errors
- **toContain**: More flexible than exact match

---

### 6. Test Structure with describe/it
**Jasmine pattern:** Organize tests with nested describe blocks.

**Example:**
```typescript
describe('App', () => {  // Component name
  beforeEach(async () => {
    // Setup
  });

  it('should create the app', () => {
    // Test 1
  });

  it('should have the correct title', () => {
    // Test 2
  });

  it('should render title', () => {
    // Test 3
  });
});
```

**Convention:**
- **Top-level describe**: Named after component/class being tested
- **beforeEach for setup**: Common setup in beforeEach block
- **Descriptive `it` blocks**: Clear test descriptions
- **One assertion per test** (preferred but not strict)

---

### 7. Fixture Management Pattern
**Standard pattern:** Create fixture, extract component instance.

**Example:**
```typescript
it('test name', () => {
  const fixture = TestBed.createComponent(App);
  const app = fixture.componentInstance;
  
  // Test the app instance
  expect(app.title()).toEqual('roman-redis-manager-ui');
});
```

**Convention:**
- **fixture variable**: ComponentFixture from TestBed
- **instance variable**: Extract componentInstance
- **Name after component**: Use descriptive name (e.g., `app`, `dashboard`)
- **Per-test creation**: Create fresh fixture in each test (or beforeEach)

---

### 8. Expected PrimeNG Testing Pattern
**Future pattern:** When testing components with PrimeNG.

**Example:**
```typescript
import { TestBed } from '@angular/core/testing';
import { ButtonModule } from 'primeng/button';
import { MyComponent } from './my-component';

describe('MyComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent, ButtonModule],  // Include PrimeNG modules
    }).compileComponents();
  });

  // tests...
});
```

**Convention:**
- **Import PrimeNG modules**: Include in TestBed configuration
- **No providePrimeNG needed**: In tests, import modules directly
- **Test interactions**: Test button clicks, form inputs, etc.

---

## Summary Checklist

When writing tests in this codebase:

- [ ] Use `.spec.ts` file extension
- [ ] Colocate test file with source file
- [ ] Use `imports` array in TestBed (not `declarations`)
- [ ] Import standalone component directly
- [ ] Use `async/await` with `compileComponents()`
- [ ] Test signals with function call syntax `signal()`
- [ ] Always include "should create" test first
- [ ] Call `fixture.detectChanges()` before DOM assertions
- [ ] Cast `nativeElement` to `HTMLElement`
- [ ] Use optional chaining `?.` when querying DOM
- [ ] Organize with `describe` and `it` blocks
- [ ] Create fresh fixture for each test or in `beforeEach`
- [ ] Import PrimeNG modules in test configuration when needed
