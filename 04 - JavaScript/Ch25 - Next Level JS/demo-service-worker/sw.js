/**
 * @file sw.js
 * @description Service Worker script demonstrating the Cache-First offline shell strategy.
 */

// Cache version identifier: increment when modifying assets to bust old caches!
const CACHE_NAME = 'cse213-app-shell-v1';

// Static resources to cache immediately during installation
const STATIC_ASSETS = [
  './index.html'
];

/**
 * 1. INSTALL EVENT
 * Fires once when the browser installs the Service Worker.
 * Pre-caches the essential application shell files.
 */
self.addEventListener('install', event => {
  console.log('[Service Worker] Installing...');
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => {
      console.log('[Service Worker] Caching App Shell assets');
      return cache.addAll(STATIC_ASSETS);
    })
  );
  // Force active state immediately without waiting for existing tabs to close
  self.skipWaiting();
});

/**
 * 2. ACTIVATE EVENT
 * Cleans up outdated cache buckets from previous versions.
 */
self.addEventListener('activate', event => {
  console.log('[Service Worker] Activating & cleaning old caches...');
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames
          .filter(name => name !== CACHE_NAME)
          .map(name => caches.delete(name))
      );
    })
  );
  // Claim control of all clients immediately
  self.clients.claim();
});

/**
 * 3. FETCH EVENT
 * Intercepts every outgoing HTTP network request from the page.
 * Implements "Cache-First" strategy:
 * If found in cache -> return cached copy immediately.
 * Otherwise -> fetch from the network.
 */
self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request).then(cachedResponse => {
      if (cachedResponse) {
        console.log('[Service Worker] Serving from cache:', event.request.url);
        return cachedResponse;
      }
      return fetch(event.request);
    })
  );
});
