import { products } from './data.js';
import { filterProducts, productNames, totalPrice } from './utils.js';

const category = document.querySelector('#category');
const list = document.querySelector('#products');
const total = document.querySelector('#total');

// The display code previews chapter 24. Focus first on the pure functions in utils.js.
function render() {
  const selected = filterProducts(products, category.value);
  list.replaceChildren();
  for (const name of productNames(selected)) {
    const item = document.createElement('li');
    item.textContent = name;
    list.append(item);
  }
  total.textContent = `Total: €${totalPrice(selected).toFixed(2)}`;
}

category.addEventListener('change', render);
render();
