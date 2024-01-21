export const load = async ({ fetch }) => {
	var d = new Date().toISOString().slice(0,10);
	const averageTimeProjection = {
		store: "1",
		currentNumberOfVisitors: 0,
		totalOfVisitors: 0,
		totalTime: "",
		averageTime: ""
	};
	return {
		averageTimeProjection: averageTimeProjection,
		date: d
	};
};