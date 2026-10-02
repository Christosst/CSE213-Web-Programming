import { api } from './api.js';
const form = document.querySelector('#text-form');
const input = document.querySelector('#text');
const status = document.querySelector('#status');
const result = document.querySelector('#result');
const buttons = form.querySelectorAll('button');

form.addEventListener('submit', async function (event) {
  event.preventDefault();
  const operation = event.submitter?.value ?? 'analysis';
  status.textContent = 'Processing...';
  result.textContent = '';
  for (const button of buttons) button.disabled = true;
  try {
    const data = await api(`/api/text/${operation}`, {
      method: 'POST',
      body: JSON.stringify({ text: input.value })
    });
    result.textContent = JSON.stringify(data, null, 2);
    status.textContent = 'Done.';
  } catch (error) {
    status.textContent = `Could not process text: ${error.message}`;
  } finally {
    for (const button of buttons) button.disabled = false;
  }
});
