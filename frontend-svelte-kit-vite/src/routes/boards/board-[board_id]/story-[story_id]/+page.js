export const load = async ({ fetch, params }) => {
	console.log('params', params);
	const productRes = await fetch('https://dummyjson.com/products/' + params.board_id);
	const product = await productRes.json();

	console.log('product', product);
	return {
		product: product
	};
};
