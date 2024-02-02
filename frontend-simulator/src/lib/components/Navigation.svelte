<script>
	import { getDrawerStore } from '@skeletonlabs/skeleton';

	const drawerStore = getDrawerStore();

	function drawerClose() {
		drawerStore.close();
	}
</script>

<nav class="list-nav p-4">
	<ul>
		<li><a href="/" on:click={drawerClose}>Home</a></li>
		<li><a href="/stores" on:click={drawerClose}>Stores</a></li>
		<li><a href="/projections" on:click={drawerClose}>Projections</a></li>
		<li><a href="/sensors" on:click={drawerClose}>Sensors</a></li>
	</ul>
</nav>
	