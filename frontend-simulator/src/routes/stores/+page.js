export const load = async ({ fetch }) => {
	var d = new Date().toISOString().slice(0,10);
	const storesResult = await fetch(`http://localhost:4000/queries/stores/${d}`);
	// const storesResult = await fetch(`http://localhost:2113/projection/myProjection/state`);
	const storesData = await storesResult.json();
	// console.log('storesData', storesData);
	return {
		stores: storesData,
		date: d
	};
};