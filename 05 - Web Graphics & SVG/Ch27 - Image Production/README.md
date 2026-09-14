# Ch27 — Image Production
**Slide deck:** LWD6_CH27_ImageProduction.pdf

## Demo files
| File | Concept |
|------|---------|
| demo-image-optimisation.html | Lighthouse audit, explicit dimensions (CLS fix), lazy loading, srcset |

## Live coding notes
1. Open Lighthouse without optimisations — note CLS and image warnings.
2. Add width + height — CLS disappears.
3. Add loading="lazy" — images below fold defer.
4. Add srcset — smaller file fetched at narrow viewport.
