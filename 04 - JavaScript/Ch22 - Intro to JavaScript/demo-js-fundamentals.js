const form = document.querySelector('#scores');
const result = document.querySelector('#result');

form.addEventListener('submit', function (event) {
  event.preventDefault();
  const firstText = document.querySelector('#first').value;
  const secondText = document.querySelector('#second').value;
  const first = Number(firstText);
  const second = Number(secondText);
  // Put a breakpoint here. Without Number(), + concatenates strings.
  const average = (first + second) / 2;
  result.textContent = `Practice average: ${average.toFixed(1)}`;
});
