export const load = async ({ fetch }) => {
	const productRes = await fetch('https://dummyjson.com/products?limit=10');
	const prodData = await productRes.json();
	return {
		products: prodData.products
	};
};

function getHighestNumber(a, b) {
	// Id a and b are equal return null	
	if (a === b) {		
		return null;}

	return a > b ? a : b;
}