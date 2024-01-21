<script>
	import { layoutContent, isEmbedded } from '../lib/navStore.js';
	layoutContent.set(null);
	isEmbedded.set(false);

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
		{#if $isEmbedded}
		<!-- No nav in embedded mode -->
		{:else}
		<AppBar>
			<svelte:fragment slot="lead">
				<button class="btn btn-sm mr-4" on:click={drawerOpen}>
					<svg viewBox="0 0 100 80" class="fill-token w-4 h-4">
						<rect width="100" height="20" />
						<rect width="100" height="20" y="30" />
						<rect width="100" height="20" y="60" />
					</svg>
				</button>
				<!-- <strong class="text-xl uppercase">L&#9901;VE/H&#9760;TE Evented</strong> -->
				
				{#if $layoutContent}
					{#each $layoutContent as laylink}
						<strong class="text-xl">{laylink}</strong>
					{/each}
				{:else}
					<strong class="text-xl">Retail Rhythm Radar - Simulator</strong>
				{/if}
			</svelte:fragment>
			<svelte:fragment slot="trail">
				<nav>
					<a href="/">Home</a>
					<a href="/stores">Stores</a>
					<a href="/projections">Projections</a>
					<a href="/sensors">Sensors</a>
				</nav>
			</svelte:fragment>
		</AppBar>
		{/if}
	</svelte:fragment>
<!-- 	
	<svelte:fragment slot="sidebarLeft">
		<Navigation />
	</svelte:fragment> -->

	<!-- (sidebarRight) -->
	
	<!-- <svelte:fragment slot="pageHeader">Page Header</svelte:fragment> -->

	<!-- Router Slotp-10 -->
	<div class="container h-full mx-auto">
		<div class="space-y-5">
			<slot />
		</div>
	</div>
<!-- 	
	<svelte:fragment slot="footer">Footer</svelte:fragment> -->
</AppShell>
