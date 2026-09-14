/**
 * @file utils.js
 * @description Pure utility functions for array processing and HTML template generation.
 * All functions here are PURE: they do not modify the original array (no side effects).
 */

/**
 * Filters an array of services by category.
 * Demonstrates: Array.prototype.filter()
 *
 * @param {Array} items - Array of service objects
 * @param {string} category - Category name ('All', 'Academic', etc.)
 * @returns {Array} Filtered subset of items
 */
export const filterByCategory = (items, category) => {
  if (category === 'All') {
    return items;
  }
  // filter() returns a NEW array containing only items where callback returns true
  return items.filter(service => service.category === category);
};

/**
 * Sorts services alphabetically by name.
 * Demonstrates: Array spread operator [...items] and localeCompare()
 *
 * WHY SPREAD FIRST?
 * Array.prototype.sort() mutates the array in-place!
 * Creating a copy with `[...items]` prevents unexpected bugs elsewhere in the app.
 *
 * @param {Array} items - Array of service objects
 * @returns {Array} Sorted copy of items
 */
export const sortByName = items => {
  return [...items].sort((itemA, itemB) => itemA.name.localeCompare(itemB.name));
};

/**
 * Transforms a service object into an accessible HTML card string.
 * Demonstrates: Template literals (backticks) and interpolation (${...})
 *
 * @param {Object} service - Single service record
 * @returns {string} HTML snippet string
 */
export const createServiceCardHTML = service => `
  <article class="service-card">
    <span class="service-tag">${service.category}</span>
    <h3>${service.name}</h3>
    <p class="service-rating">★ Rating: ${service.rating} / 5.0</p>
  </article>
`;
