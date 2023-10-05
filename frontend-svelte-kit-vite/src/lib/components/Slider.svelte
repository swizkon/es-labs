<script>
	import { RangeSlider } from '@skeletonlabs/skeleton';

	// import { onMount } from 'svelte';
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher();

	export let label;
	export let key;
	export let value;

	let active = false;

	$: newValue = value;

	$: newValue && active && handleLevelChangedEvent();

	function handleLevelChangedEvent() {
		console.log('key', key, typeof key);
		console.log('newValue', newValue, typeof newValue);
		dispatch('sliderChange', {
			key: key,
			value: newValue
		});
	}

	// onMount(async () => {
	// 	active = true;
	// });

	// function dispatchLevelChangedEvent(val) {
	// 	dispatch('sliderChange', {
	// 		key: key,
	// 		value: val
	// 	});
	// }
	// on:input={(e) => dispatchLevelChangedEvent(e.target.value)}
</script>

<RangeSlider name="range-slider" bind:value max={50} step={1} on:click={() => (active = true)}>
	<div class="flex justify-between items-center">
		<div class="font-bold">{label}</div>
		<div class="text-xs">{value} / {50}</div>
	</div>
</RangeSlider>
