/**
 * @file main.js
 * @description Main application controller. Orchestrates data, logic, and DOM events.
 */

// 1. IMPORT MODULE DEPENDENCIES
import { services } from './data.js';
import { filterByCategory, sortByName, createServiceCardHTML } from './utils.js';

// 2. CACHE DOM ELEMENT REFERENCES
const container = document.getElementById('service-cards-container');
const filterControls = document.getElementById('filter-controls');

/**
 * Functional rendering pipeline:
 * Takes a category -> Filters data -> Sorts alphabetically -> Converts to HTML -> Updates DOM
 * @param {string} category - The selected filter category
 */
function renderCards(category) {
  // 1. Apply filtering logic
  const filtered = filterByCategory(services, category);

  // 2. Apply alphabetical sorting
  const sorted = sortByName(filtered);

  // 3. Map array of objects to array of HTML strings, then join into one single string
  // This minimizes browser DOM repaints by setting innerHTML only ONCE!
  container.innerHTML = sorted.map(createServiceCardHTML).join('');
}

// 3. ATTACH EVENT LISTENERS (EVENT DELEGATION PATTERN)
// Instead of adding click listeners to 4 separate buttons,
// we attach ONE listener to the parent container:
filterControls.addEventListener('click', event => {
  // Find the closest button that has the data-category attribute
  const clickedButton = event.target.closest('[data-category]');
  if (!clickedButton) return; // User clicked the whitespace between buttons

  // Update active styling
  document.querySelectorAll('#filter-controls .filter-btn').forEach(btn => {
    btn.classList.remove('active');
  });
  clickedButton.classList.add('active');

  // Trigger re-render with newly selected category
  const selectedCategory = clickedButton.dataset.category;
  renderCards(selectedCategory);
});

// 4. INITIALIZE PAGE WITH "All" CATEGORY
renderCards('All');
