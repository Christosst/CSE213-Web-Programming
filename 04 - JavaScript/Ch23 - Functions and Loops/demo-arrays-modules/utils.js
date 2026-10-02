export function filterProducts(products, category) {
  if (category === 'all') return products;
  return products.filter(function (product) {
    return product.category === category;
  });
}

export function productNames(products) {
  return products.map(function (product) {
    return product.name;
  });
}

export function totalPrice(products) {
  return products.reduce(function (total, product) {
    return total + product.price;
  }, 0);
}

// Compare this loop with reduce. Both return the same total.
export function totalWithLoop(products) {
  let total = 0;
  for (const product of products) {
    total += product.price;
  }
  return total;
}
