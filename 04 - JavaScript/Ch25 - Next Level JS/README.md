# Ch25 — Next Level JS
**Slide deck:** LWD6_Ch25_NextlevelJS.pdf

## Demo files
| File | Concept |
|------|---------|
| demo-fetch-states.html | fetch + async/await: loading / success / empty / error states |
| demo-web-worker/      | Web Worker: blocking vs non-blocking computation |
| demo-service-worker/  | Service Worker: offline app shell |
| demo-local-storage.html | localStorage: persist favourites across reloads |

## Live coding notes
- fetch: throttle to Slow 3G to see spinner; disable network to see error state + retry.
- worker: watch the CSS spinner freeze on main thread, stay smooth in worker.
- service-worker: go offline in DevTools, reload — page still loads.
- localStorage: toggle favourites, reload — selection persists.
