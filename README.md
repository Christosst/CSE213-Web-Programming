# CSE213 — Instructor Demos

Live-coding demos for all 28 LWD6 chapters, organised to match the slide deck folder structure.

## Folder Structure

```
Instructor Demos/
├── 01 - Introduction/
│   ├── Ch01 - Get Started/           demo-first-page.html
│   ├── Ch02 - How the Web Works/     demo-devtools-network.html
│   └── Ch03 - Big Concepts/          demo-progressive-enhancement.html
├── 02 - HTML/
│   ├── Ch04 - HTML Overview/         demo-semantic-vs-div.html
│   ├── Ch05 - Text/                  demo-headings-and-text.html
│   ├── Ch06 - Links/                 demo-links.html
│   ├── Ch07 - Images/                demo-images.html
│   ├── Ch08 - Tables/                demo-tables.html
│   ├── Ch09 - Forms/                 demo-accessible-form.html
│   ├── Ch10 - Media/                 demo-media.html
│   └── ARTICLE - Responsive Images/  demo-responsive-images.html
├── 03 - CSS/
│   ├── Ch11 - Intro to CSS/          demo-intro-css.html
│   ├── Ch12 - Formatting Text/       demo-typography.html
│   ├── Ch13 - Colors and Backgrounds/demo-colors.html
│   ├── Ch14 - Selectors/             demo-selectors.html
│   ├── Ch15 - Box Model/             demo-box-model.html
│   ├── Ch16 - Floating and Positioning/ demo-float-position.html
│   ├── Ch17 - Flexbox/               demo-flexbox.html
│   ├── Ch18 - Grid/                  demo-grid.html
│   ├── Ch19 - Responsive Web Design/ demo-responsive.html
│   ├── Ch20 - Transitions and Animation/ demo-animation.html
│   └── Ch21 - CSS Techniques/        demo-css-techniques.html
├── 04 - JavaScript/
│   ├── Ch22 - Intro to JavaScript/   demo-js-fundamentals.html
│   ├── Ch23 - Functions and Loops/   demo-arrays-modules/ (4 files)
│   ├── Ch24 - DOM and Events/        demo-dom-events.html
│   └── Ch25 - Next Level JS/
│       ├── demo-fetch-states.html
│       ├── demo-local-storage.html
│       ├── demo-web-worker/          (index.html + worker.js)
│       └── demo-service-worker/      (index.html + sw.js)
└── 05 - Web Graphics & SVG/
    ├── Ch26 - Web Image Basics/      demo-image-formats.html
    ├── Ch27 - Image Production/      demo-image-optimisation.html
    └── Ch28 - SVG/                   demo-svg.html
```

## How to Use

1. Open any `.html` file directly in the browser (double-click or drag to Chrome/Edge).
2. For ES module demos (Ch23) and Service Worker (Ch25), serve via a local server:
   - VS Code: **Live Server** extension, or
   - Terminal: `npx serve .` inside the demo folder.
3. Each chapter folder has a `README.md` with live coding notes and DevTools steps.

## Quick Reference — DevTools Moments

| Chapter | Key DevTools panel |
|---|---|
| Ch02 | Network — HTTP request/response headers |
| Ch04 | Accessibility — landmark regions |
| Ch07 | Network — throttle + image file sizes |
| Ch09 | Accessibility — accessible name inspection |
| Ch14 | Elements → Styles — winning declarations |
| Ch15 | Elements → Box model diagram |
| Ch19 | Device Toolbar — responsive breakpoints |
| Ch20 | Rendering — Emulate prefers-reduced-motion |
| Ch22 | Sources — breakpoints, Scope panel |
| Ch23 | Sources — Module graph |
| Ch25 | Application — Service Workers, Cache Storage |
| Ch26 | Network — image transfer sizes |
| Ch27 | Lighthouse — performance audit |
