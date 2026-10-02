const button = document.querySelector('#greet-btn');
const message = document.querySelector('#message');

button.addEventListener('click', function () {
  message.textContent = 'Hello! JavaScript is working.';
});
