export const load = async ({ fetch }) => {
	var d = new Date().toISOString().slice(0,10);
	const storesResult = await fetch(`http://localhost:5248/StoreFlow/queries/stores/${d}`);
	const storesData = await storesResult.json();
	// console.log('storesData', storesData);
	return {
		stores: storesData,
		date: d
	};
};