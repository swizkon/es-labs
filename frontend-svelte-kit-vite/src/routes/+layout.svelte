<script>
	import { layoutContent } from '../lib/navStore.js';
	layoutContent.set(null);

	import { AppShell, AppBar, Drawer, getDrawerStore } from '@skeletonlabs/skeleton';
	import { Toast } from '@skeletonlabs/skeleton';

	import { initializeStores } from '@skeletonlabs/skeleton';
	initializeStores();

	// import '@skeletonlabs/skeleton';
	import '../app.postcss';
	import Navigation from '../lib/components/Navigation.svelte';
	const drawerStore = getDrawerStore();

	function drawerOpen() {
		drawerStore.open();
	}
</script>

<Drawer>
	<Navigation />
</Drawer>

<Toast />

<AppShell slotSidebarLeft="w-0 md:w-52 bg-surface-500/10">
	<svelte:fragment slot="header">
		<AppBar>
			<svelte:fragment slot="lead">
				<button class="btn btn-sm mr-4" on:click={drawerOpen}>
					<svg viewBox="0 0 100 80" class="fill-token w-4 h-4">
						<rect width="100" height="20" />
						<rect width="100" height="20" y="30" />
						<rect width="100" height="20" y="60" />
					</svg>
				</button>
				<strong class="text-xl uppercase">Evented</strong>
			</svelte:fragment>
			<svelte:fragment slot="trail">
				<nav>
					{#if $layoutContent}
						{#each $layoutContent as laylink}
							<a class="p-1" href={laylink}>{laylink}</a>
						{/each}
					{/if}
					<a href="/">Home</a>
					<a href="/stores">stores</a>
					<a href="/about">about</a>
					<a href="/boards">boards</a>
				</nav>
			</svelte:fragment>
		</AppBar>
	</svelte:fragment>
	<svelte:fragment slot="sidebarLeft">
		<Navigation />
	</svelte:fragment>
	<!-- (sidebarRight) -->
	<svelte:fragment slot="pageHeader">Page Header</svelte:fragment>
	<!-- Router Slot -->
	<div class="container p-10 h-full mx-auto">
		<div class="space-y-5">
			<slot />
		</div>
	</div>
	<svelte:fragment slot="footer">Footer</svelte:fragment>
</AppShell>
