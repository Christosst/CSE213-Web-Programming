# ARTICLE — Responsive Images
**Slide deck:** LWD_ARTICLE_responsive_images.pdf

## Demo files
| File | Concept |
|------|---------|
| demo-responsive-images.html | srcset/sizes, <picture>, AVIF/WebP fallback, lazy loading |

## Live coding notes
1. Start with plain <img> — no srcset, no explicit dimensions.
2. Network throttle to Slow 3G — show which file downloads.
3. Add srcset — refresh — show the smaller file is requested at narrow viewport.
4. Add <picture> + <source type='image/avif'> — show AVIF loads if browser supports it.
