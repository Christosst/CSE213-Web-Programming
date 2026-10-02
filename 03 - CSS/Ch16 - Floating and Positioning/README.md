# Chapter 16: Wrapping and positioning

Open `demo-float-position.html` from the course index after running `npm start` at the repository root. Read the HTML first; then inspect the relevant CSS or JavaScript. The styling around the example is only for readability.

## Instructor demonstration (about 12–15 minutes)

1. Toggle float and flow-root to compare text wrapping and containment.
2. Remove position:relative from the card and explain where the badge moves.
3. Scroll to inspect sticky and its top threshold.

## Student exercise (about 20–22 minutes)

Build a sticky header and a badge attached to its positioned parent. Add enough content to test scrolling.

## Show briefly

Fixed modal layers and stacking contexts. A modal exercise requires focus handling; see the complete optional dialog example.

Open `demo-dialog.html` for the fixed-layer extension. Native showModal puts the dialog in the top layer, moves focus inside and makes background content unavailable. Inspect ::backdrop; Escape or Close returns focus to its trigger. This is a brief extension after the relative/absolute/sticky examples.
