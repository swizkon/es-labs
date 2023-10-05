export const load = async ({ fetch }) => {
	const productRes = await fetch('https://dummyjson.com/products?limit=10');
	const prodData = await productRes.json();
	return {
		products: prodData.products
	};
};
