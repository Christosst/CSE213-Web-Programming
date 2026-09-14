# Ch09 — Forms
**Slide deck:** LWD6_Ch09_Forms.pdf

## Demo files
| File | Concept |
|------|---------|
| demo-accessible-form.html | Labels, fieldset/legend, native constraints, ARIA error messages |

## Live coding notes
1. Remove <label for> — show how the input loses its accessible name.
2. Submit empty form — browser native validation fires.
3. Add role='alert' aria-live='polite' to error div — add JS to populate it.
4. Keyboard test: Tab through every field, verify focus ring is visible.
