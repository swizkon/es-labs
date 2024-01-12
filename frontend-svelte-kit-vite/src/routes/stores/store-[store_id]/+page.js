export const load = async ({ fetch, params }) => {
	const storeResult = await fetch(`http://localhost:5248/StoreFlow/queries/store-${params.store_id}/2024-01-12`);
	const storeData = await storeResult.json();
	console.log('storeData', storeData);
	return {
		title: `Store ${params.store_id}`,
		stores: storeData
	};
};