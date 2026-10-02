const buttons = document.querySelectorAll('button');
const status = document.querySelector('#status');
const list = document.querySelector('#users');

async function loadUsers(file) {
  status.textContent = 'Loading...';
  list.replaceChildren();
  for (const button of buttons) button.disabled = true;
  try {
    const response = await fetch(file);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    const users = await response.json();
    for (const user of users) {
      const item = document.createElement('li');
      item.textContent = user.name;
      list.append(item);
    }
    status.textContent = users.length ? 'Users loaded.' : 'No users found.';
  } catch (error) {
    status.textContent = `Could not load users: ${error.message}. Try again.`;
  } finally {
    for (const button of buttons) button.disabled = false;
  }
}

for (const button of buttons) {
  button.addEventListener('click', function () {
    loadUsers(button.dataset.file);
  });
}
