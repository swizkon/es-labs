
<script>
	import { onMount, onDestroy } from 'svelte';
	import { layoutContent } from '../../lib/navStore';
	
	// $: ({ store, title, zones } = data);
    
	onMount(async () => {
		layoutContent.set(['Sensors sims']);

		return () => {
		};
	});

	onDestroy( () => {
		layoutContent.set(['Retail Rhythm Radar - Simulator']);
    });

</script>
<!-- 
<h1 class="h1 gradient-heading">Sensors for simulation</h1> -->

<div class="logo-cloud grid-cols-1 lg:!grid-cols-3 gap-1">
    <a class="logo-item" href="/sensors/enterandexit">
        <span>Store: enter/exit</span>
    </a>
</div>
