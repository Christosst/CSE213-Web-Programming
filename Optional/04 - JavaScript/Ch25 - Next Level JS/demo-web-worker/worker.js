/**
 * @file worker.js
 * @description Dedicated Web Worker script running in an isolated background thread.
 *
 * IMPORTANT WEB WORKER RULES:
 * 1. NO DOM ACCESS: Workers do NOT have access to `window`, `document`, or DOM elements!
 * 2. SEPARATE GLOBAL SCOPE: `self` refers to the WorkerGlobalScope.
 * 3. COMMUNICATION: Communicate with the main script solely via `postMessage()` and `onmessage`.
 */

self.onmessage = function (e) {
  const limit = e.data;
  const startTime = performance.now();

  // Heavy CPU Sieve of Eratosthenes computation
  const sieve = new Uint8Array(limit + 1).fill(1);
  sieve[0] = sieve[1] = 0;

  for (let i = 2; i * i <= limit; i++) {
    if (sieve[i]) {
      for (let j = i * i; j <= limit; j += i) {
        sieve[j] = 0;
      }
    }
  }

  const primeCount = sieve.reduce((sum, val) => sum + val, 0);
  const duration = (performance.now() - startTime).toFixed(1);

  // Send results object back to main thread
  self.postMessage({ primeCount, duration });
};
