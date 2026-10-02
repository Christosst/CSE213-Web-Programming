const form = document.querySelector('#scores');
const result = document.querySelector('#result');

form.addEventListener('submit', function (event) {
  event.preventDefault();
  const firstText = document.querySelector('#first').value;
  const secondText = document.querySelector('#second').value;
  const first = firstText; // Intentional lab bug: inspect typeof first.
  const second = secondText;
  // Put a breakpoint here. Without Number(), + concatenates strings.
  const average = (first + second) / 2;
  result.textContent = `Practice average: ${average.toFixed(1)}`;
});
