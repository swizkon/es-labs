<script>
	import { Drawer, getDrawerStore } from '@skeletonlabs/skeleton';

	const drawerStore = getDrawerStore();

	function drawerClose(params) {
		drawerStore.close();
	}
</script>

<nav class="list-nav p-4">
	<ul>
		<li><a href="/" on:click={drawerClose}>Home</a></li>
		<li><a href="/about" on:click={drawerClose}>about</a></li>
		<li><a href="/boards" on:click={drawerClose}>boards</a></li>
	</ul>
</nav>
