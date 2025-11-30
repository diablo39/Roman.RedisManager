# Style Guide: Component Styles

## Unique Patterns in This Codebase

### 1. Separate CSS Files for All Components
**Strict pattern:** Every component has its own `.css` file, no inline styles.

**Example:**
```typescript
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css'  // Always external CSS file
})
```

**Convention:**
- **Never inline**: Don't use `styles: [...]` in component decorator
- **Always styleUrl**: Use `styleUrl` property pointing to `.css` file
- **Colocated**: CSS file in same directory as component
- **Matching names**: `app.ts` → `app.css`

---

### 2. :host Selector for Component Root
**Standard but important:** Use `:host` to style the component's host element.

**Expected pattern:**
```css
:host {
  display: block;
  /* Host element styling */
}
```

**Convention:**
- **Block-level by default**: Set `display: block` if needed
- **CSS custom properties**: Define component-specific variables in `:host`
- **Encapsulation**: Styles in `:host` only affect the component root

---

### 3. CSS Custom Properties for Theming
**Project pattern:** Use CSS custom properties (variables) for themeable values.

**Example from app template (should be in CSS file):**
```css
:host {
  --bright-blue: oklch(51.01% 0.274 263.83);
  --electric-violet: oklch(53.18% 0.28 296.97);
  --french-violet: oklch(47.66% 0.246 305.88);
  --vivid-pink: oklch(69.02% 0.277 332.77);
  --hot-red: oklch(61.42% 0.238 15.34);
  --orange-red: oklch(63.32% 0.24 31.68);
  
  --gray-900: oklch(19.37% 0.006 300.98);
  --gray-700: oklch(36.98% 0.014 302.71);
  --gray-400: oklch(70.9% 0.015 304.04);
}
```

**Convention:**
- **Kebab-case names**: Use lowercase with hyphens
- **Semantic naming**: Use color names that describe purpose
- **Component scope**: Define variables in component's `:host`
- **Use throughout**: Reference with `var(--variable-name)`

---

### 4. OKLCH Color Space
**Modern pattern:** Colors defined using OKLCH color space.

**Example:**
```css
:host {
  --primary-color: oklch(51.01% 0.274 263.83);
  /* Format: oklch(lightness% chroma hue) */
}

.button {
  background-color: var(--primary-color);
}
```

**Convention:**
- **OKLCH format**: `oklch(L% C H)` 
  - L = Lightness (0-100%)
  - C = Chroma (0-0.4 typically)
  - H = Hue (0-360 degrees)
- **Perceptually uniform**: Better than RGB/HSL for consistency
- **Better interpolation**: Gradients look more natural

---

### 5. CSS Custom Properties for Gradients
**Reusability pattern:** Define gradients as custom properties.

**Example:**
```css
:host {
  --red-to-pink-to-purple-vertical-gradient: linear-gradient(
    180deg,
    var(--orange-red) 0%,
    var(--vivid-pink) 50%,
    var(--electric-violet) 100%
  );
  
  --red-to-pink-to-purple-horizontal-gradient: linear-gradient(
    90deg,
    var(--orange-red) 0%,
    var(--vivid-pink) 50%,
    var(--electric-violet) 100%
  );
}

.divider {
  background: var(--red-to-pink-to-purple-vertical-gradient);
}
```

**Convention:**
- **Named gradients**: Create reusable gradient definitions
- **Include direction**: Name indicates vertical/horizontal/radial
- **Use color variables**: Reference other custom properties in gradients

---

### 6. Typography Variables
**Consistent fonts:** Define font stacks as custom properties (when needed).

**Example:**
```css
:host {
  font-family: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
    Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji",
    "Segoe UI Symbol";
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
```

**Convention:**
- **Inter as primary**: Use "Inter" font family
- **System fallbacks**: Include system fonts as fallbacks
- **Emoji support**: Include emoji fonts at end
- **Font smoothing**: Enable antialiasing for better rendering

---

### 7. Avoiding Global Styles
**Encapsulation pattern:** Keep component styles scoped, minimize global CSS.

**Convention:**
- **Component-scoped by default**: Angular's view encapsulation handles this
- **Avoid global selectors**: Don't target global elements unless in `styles.css`
- **Self-contained components**: Each component brings its own styles
- **Global styles minimal**: Reserve `src/styles.css` for truly global needs

---

### 8. PrimeNG Style Integration
**Framework integration:** Rely on PrimeNG's theme, avoid overriding unless necessary.

**Convention:**
- **Use theme variables**: PrimeNG provides CSS variables for theming
- **Minimal overrides**: Only override when absolutely needed
- **Follow PrimeNG patterns**: Use their class names and structure
- **Test theme compatibility**: Ensure custom styles work with PrimeNG theme

---

## Summary Checklist

When creating component styles in this codebase:

- [ ] Create separate `.css` file for each component
- [ ] Use `styleUrl` in component decorator (never `styles`)
- [ ] Use `:host` selector for component root styling
- [ ] Define CSS custom properties for themeable values
- [ ] Use OKLCH color space for all colors
- [ ] Create gradient custom properties for reuse
- [ ] Use Inter font family with system fallbacks
- [ ] Enable font smoothing (antialiased)
- [ ] Keep styles scoped to component
- [ ] Minimal global styles (use `styles.css` sparingly)
- [ ] Respect PrimeNG theme, avoid unnecessary overrides
