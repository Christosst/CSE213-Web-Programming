const input = document.querySelector('#username');
const status = document.querySelector('#status');
const key = 'cse213.practice.username';

try {
  input.value = localStorage.getItem(key) ?? '';
} catch {
  status.textContent = 'Browser storage is unavailable.';
}

document.querySelector('#remember').addEventListener('submit', function (event) {
  event.preventDefault();
  try {
    localStorage.setItem(key, input.value);
    status.textContent = 'Saved in this browser.';
  } catch {
    status.textContent = 'Could not save in browser storage.';
  }
});

document.querySelector('#clear').addEventListener('click', function () {
  try {
    localStorage.removeItem(key);
    input.value = '';
    status.textContent = 'Forgotten.';
  } catch {
    status.textContent = 'Browser storage is unavailable.';
  }
});
