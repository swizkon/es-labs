export const load = async ({ fetch }) => {
	const storesResult = await fetch('http://localhost:5248/StoreFlow/queries/stores/2024-01-12');
	const storesData = await storesResult.json();
	// console.log('storesData', storesData);
	return {
		stores: storesData
	};
};