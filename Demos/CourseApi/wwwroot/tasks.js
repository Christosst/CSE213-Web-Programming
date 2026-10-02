import { api } from './api.js';
const form = document.querySelector('#task-form');
const title = document.querySelector('#title');
const list = document.querySelector('#tasks');
const status = document.querySelector('#status');
const reload = document.querySelector('#reload');

async function loadTasks() {
  const tasks = await api('/api/tasks');
  list.replaceChildren();
  for (const task of tasks) {
    const item = document.createElement('li');
    const text = document.createElement('span');
    text.textContent = `${task.title} (${task.isCompleted ? 'complete' : 'open'}) `;
    const toggle = document.createElement('button');
    toggle.type = 'button';
    toggle.textContent = task.isCompleted ? 'Reopen' : 'Complete';
    toggle.addEventListener('click', () => perform(async () => {
      await api(`/api/tasks/${task.id}`, {
        method: 'PUT',
        body: JSON.stringify({ title: task.title, isCompleted: !task.isCompleted })
      });
      await loadTasks();
    }));
    const remove = document.createElement('button');
    remove.type = 'button';
    remove.textContent = 'Delete';
    remove.addEventListener('click', () => perform(async () => {
      await api(`/api/tasks/${task.id}`, { method: 'DELETE' });
      await loadTasks();
    }));
    item.append(text, toggle, remove);
    list.append(item);
  }
  status.textContent = tasks.length ? 'Tasks loaded.' : 'No tasks yet.';
}

// Disable controls during a request, including controls recreated by loadTasks.
async function perform(action) {
  status.textContent = 'Working...';
  for (const button of document.querySelectorAll('button')) button.disabled = true;
  list.inert = true;
  try {
    await action();
  } catch (error) {
    status.textContent = `Request failed: ${error.message}`;
  } finally {
    list.inert = false;
    for (const button of document.querySelectorAll('button')) button.disabled = false;
  }
}

form.addEventListener('submit', function (event) {
  event.preventDefault();
  perform(async () => {
    await api('/api/tasks', {
      method: 'POST',
      body: JSON.stringify({ title: title.value, isCompleted: false })
    });
    form.reset();
    await loadTasks();
  });
});
reload.addEventListener('click', () => perform(loadTasks));
perform(loadTasks);
