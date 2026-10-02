# Ch20 — Transitions and Animation
**Slide deck:** LWD6_Ch20_Transitions.pdf

## Demo files
| File | Concept |
|------|---------|
| demo-animation.html | CSS transitions, @keyframes spinner, 3D card flip, prefers-reduced-motion |

## Live coding notes
1. Show the spinner running — draw attention to animation: spin 1s linear infinite.
2. Open DevTools → Rendering → Emulate prefers-reduced-motion: reduce — spinner stops.
3. Show the card flip on hover — transform: rotateY(180deg) with perspective.
4. Key message: always include prefers-reduced-motion; animation must be non-essential.
