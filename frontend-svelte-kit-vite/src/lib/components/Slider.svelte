<script>
	import { RangeSlider } from '@skeletonlabs/skeleton';

	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher();

	export let label;
	export let key;
	export let value;

	// $: newValue = value;
	$: value && handleLevelChangedEvent();

	function handleLevelChangedEvent() {
		console.log('key', key, typeof key);
		console.log('newValue', value, typeof value);
		dispatch('sliderChange', {
			key: key,
			value: value
		});
	}
</script>

<RangeSlider name="range-slider" bind:value max={50} step={1}>
	<div class="flex justify-between items-center">
		<div class="font-bold">{label}</div>
		<div class="text-xs">{value} / {50}</div>
	</div>
</RangeSlider>
