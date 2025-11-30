# Unit Tests Style Guide

## Unique Conventions

This style guide covers **project-specific testing patterns** using Jasmine and Karma.

### 1. Standalone Component Testing Pattern

**Project-specific approach:**
- Import standalone components directly in TestBed
- Use `imports` array, not `declarations`

**From `src/app/app.spec.ts`:**
```typescript
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],  // Import standalone component
    }).compileComponents();
  });
  
  // ... tests
});
```

**Key pattern:**
```typescript
// ✓ Standalone component pattern
imports: [App]

// ✗ Old module pattern (not used)
declarations: [App]
```

### 2. Async BeforeEach with Await

**Project convention:**
- Always use `async` for `beforeEach`
- Always `await` on `compileComponents()`

**Pattern:**
```typescript
beforeEach(async () => {
  await TestBed.configureTestingModule({
    imports: [App],
  }).compileComponents();
});
```

**Why:**
- Template compilation is asynchronous
- Ensures templates are ready before tests run
- Required for standalone components

**Not used:**
```typescript
// ✗ Synchronous beforeEach (unreliable)
beforeEach(() => {
  TestBed.configureTestingModule({
    imports: [App],
  });
});
```

### 3. Fixture-Based Testing

**Project pattern:**
- Create fixture for each test
- Access component instance via `fixture.componentInstance`
- Manually trigger change detection

**From `src/app/app.spec.ts`:**
```typescript
it('should create the app', () => {
  const fixture = TestBed.createComponent(App);
  const app = fixture.componentInstance;
  expect(app).toBeTruthy();
});

it('should render title', () => {
  const fixture = TestBed.createComponent(App);
  fixture.detectChanges();  // Trigger change detection
  const compiled = fixture.nativeElement as HTMLElement;
  expect(compiled.querySelector('h1')?.textContent).toContain('Hello, roman-redis-manager-ui');
});
```

**Key patterns:**
```typescript
// 1. Create fixture
const fixture = TestBed.createComponent(App);

// 2. Get component instance
const app = fixture.componentInstance;

// 3. Trigger change detection (before DOM assertions)
fixture.detectChanges();

// 4. Query DOM with type safety
const compiled = fixture.nativeElement as HTMLElement;
const element = compiled.querySelector('h1');
```

### 4. TypeScript Type Safety in Tests

**Project convention:**
- Cast `nativeElement` to `HTMLElement`
- Use optional chaining for safe property access

**Pattern:**
```typescript
const compiled = fixture.nativeElement as HTMLElement;
expect(compiled.querySelector('h1')?.textContent).toContain('Hello');
```

**Type safety benefits:**
- IntelliSense for DOM methods
- Type checking for selectors
- Compile-time error detection

### 5. Descriptive Test Names with "should"

**Project pattern:**
- All test names start with "should"
- Clear, descriptive action/expectation

**From tests:**
```typescript
it('should create the app', () => { ... });
it('should render title', () => { ... });
```

**Pattern:**
```typescript
// ✓ Descriptive with "should"
it('should display user name when loaded', () => { ... });
it('should call API on button click', () => { ... });
it('should validate form inputs', () => { ... });

// ✗ Vague or unclear
it('works', () => { ... });
it('test name', () => { ... });
```

### 6. DOM Query Strategy

**Project approach:**
- Use standard DOM query methods
- Query from `fixture.nativeElement`
- Type assertions for safety

**Pattern:**
```typescript
const compiled = fixture.nativeElement as HTMLElement;

// Query single element
const header = compiled.querySelector('h1');

// Query multiple elements
const buttons = compiled.querySelectorAll('button');

// Query by class
const container = compiled.querySelector('.main-content');

// Query by attribute
const input = compiled.querySelector('[data-testid="email"]');
```

### 7. Colocated Test Files

**Project structure:**
- Test files next to component files
- Same directory, `.spec.ts` suffix

**Structure:**
```
app/
├── app.ts
├── app.spec.ts      ← Colocated
├── app.html
└── app.css
```

**Not used:**
```
// ✗ Separate test directory
tests/
└── app.spec.ts
```

## Key Takeaways

When writing tests in this project:

1. **Imports**: Use `imports` array for standalone components
2. **Async Setup**: Always `async/await` in `beforeEach`
3. **Fixtures**: Create fixture per test, trigger change detection
4. **Type Safety**: Cast `nativeElement` to `HTMLElement`
5. **Test Names**: Use descriptive "should" statements
6. **DOM Queries**: Use standard query methods with type safety
7. **Colocation**: Keep tests next to source files

## Complete Test File Example

**Typical component test structure:**
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

  it('should display initial title', () => {
    const fixture = TestBed.createComponent(MyComponent);
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const title = compiled.querySelector('h1');
    
    expect(title?.textContent).toContain('My Component');
  });

  it('should update signal value', () => {
    const fixture = TestBed.createComponent(MyComponent);
    const component = fixture.componentInstance;
    
    component.mySignal.set('new value');
    expect(component.mySignal()).toBe('new value');
  });

  it('should respond to button click', () => {
    const fixture = TestBed.createComponent(MyComponent);
    const component = fixture.componentInstance;
    spyOn(component, 'handleClick');
    
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector('button');
    
    button?.click();
    expect(component.handleClick).toHaveBeenCalled();
  });
});
```

## Testing with Dependencies

**When component has dependencies:**
```typescript
import { TestBed } from '@angular/core/testing';
import { MyComponent } from './my-component';
import { MyService } from './my-service';
import { OtherComponent } from './other-component';

describe('MyComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent, OtherComponent],  // Import all standalone components
      providers: [MyService]  // Provide services
    }).compileComponents();
  });

  // ... tests
});
```

## Testing Signal-Based Components

**Project-specific signal testing:**
```typescript
it('should update signal and reflect in DOM', () => {
  const fixture = TestBed.createComponent(MyComponent);
  const component = fixture.componentInstance;
  
  // Update signal
  component.title.set('New Title');
  
  // Verify signal value
  expect(component.title()).toBe('New Title');
  
  // Trigger change detection
  fixture.detectChanges();
  
  // Verify DOM update
  const compiled = fixture.nativeElement as HTMLElement;
  expect(compiled.querySelector('h1')?.textContent).toContain('New Title');
});
```

## Mocking Services

**Service mock pattern:**
```typescript
const mockService = jasmine.createSpyObj('MyService', ['getData', 'saveData']);
mockService.getData.and.returnValue('mock data');

await TestBed.configureTestingModule({
  imports: [MyComponent],
  providers: [
    { provide: MyService, useValue: mockService }
  ]
}).compileComponents();
```
