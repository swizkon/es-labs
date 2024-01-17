export const load = async ({ fetch, params }) => {
	const storeResult = await fetch(`http://localhost:4000/StoreFlow/queries/store-${params.store_id}/${params.date}`);
	const storeData = await storeResult.json();
	
	let zones = [
		{
			"zone": "A",
			"visitors": 0,
			"color": 0x663399,
			"position": [-10, 0, 0],
			"size": [100, 200]
		},
		{
			"zone": "B",
			"visitors": 0,
			"color": 0x336699,
			"position": [0, 0, -7.5],
			"size": [100, 150]
		},
		{	
			"zone": "C",
			"visitors": 0,
			"color": 0x996633,
			"position": [0, 0, 7.5],
			"size": [100, 150]
		},
		{
			"zone": "D",
			"visitors": 0,
			"color": 0x339966,
			"position": [10, 0, 0],
			"size": [100, 200]
		}
	];

	zones = zones.map((z) => {
		const zoneVisitor = storeData.zoneVisitor[z.zone];
		if (zoneVisitor) {
			z.visitors = zoneVisitor;
		}
		return z;
	});

	return {
		store: params.store_id,
		title: `Store ${params.store_id}`,
		zones: zones
	};
};