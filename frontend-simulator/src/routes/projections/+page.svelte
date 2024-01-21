
<script>
	import { onMount, onDestroy } from 'svelte';
	import { layoutContent } from '../../lib/navStore';
	
	// $: ({ store, title, zones } = data);
    
	onMount(async () => {
		layoutContent.set(['Projections']);

		return () => {
		};
	});

	onDestroy( () => {
		layoutContent.set(['']);
    });

</script>
<!-- 
<h1 class="h1 gradient-heading">Sensors for simulation</h1> -->

<div class="logo-cloud grid-cols-1 lg:!grid-cols-3 gap-1">
    <a class="logo-item" href="/projections/avgtime">
        <span>avgtime</span>
    </a>
</div>
