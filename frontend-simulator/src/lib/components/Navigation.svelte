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
		<li><a href="/stores" on:click={drawerClose}>stores</a></li>
		<li><a href="/projections" on:click={drawerClose}>Projections</a></li>
		<li><a href="/sensors" on:click={drawerClose}>sensors</a></li>
	</ul>
</nav>
	