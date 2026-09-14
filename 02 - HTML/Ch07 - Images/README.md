# Ch07 — Images
**Slide deck:** LWD6_Ch07_Images.pdf

## Demo files
| File | Concept |
|------|---------|
| demo-images.html | alt text decisions, width/height, lazy loading, CLS prevention |

## Live coding notes
1. Remove alt — DevTools Accessibility shows 'Image has no alt attribute'.
2. Remove width/height — throttle to Slow 3G, show layout shift (CLS).
3. Add loading='lazy' — observe images load only when scrolled into view.
