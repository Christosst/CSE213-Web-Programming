const form = document.querySelector('#task-form');
const title = document.querySelector('#title');
const list = document.querySelector('#tasks');
const status = document.querySelector('#status');

form.addEventListener('submit', function (event) {
  event.preventDefault();
  if (title.value.trim() === '') {
    status.textContent = 'Enter a task title.';
    return;
  }
  const item = document.createElement('li');
  const label = document.createElement('label');
  const checkbox = document.createElement('input');
  checkbox.type = 'checkbox';
  const text = document.createElement('span');
  text.textContent = title.value.trim();
  label.append(checkbox, text);
  const remove = document.createElement('button');
  remove.type = 'button';
  remove.textContent = 'Delete';
  label.append(' ');
  item.append(label, remove);
  list.append(item);
  form.reset();
  title.focus();
  status.textContent = 'Task added. Tasks reset when this page reloads.';
});

// One listener handles controls in every current and future list item.
list.addEventListener('click', function (event) {
  const item = event.target.closest('li');
  if (!item) return;
  if (event.target.matches('button')) {
    item.remove();
    status.textContent = list.children.length ? 'Task deleted.' : 'No tasks yet.';
  }
  if (event.target.matches('input[type="checkbox"]')) {
    item.classList.toggle('completed', event.target.checked);
  }
});
