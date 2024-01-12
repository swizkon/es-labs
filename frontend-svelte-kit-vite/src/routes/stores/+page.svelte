<script>
	import Link from '../../components/base/Link.svelte';
    import { ProgressRadial } from '@skeletonlabs/skeleton';
	export let data;
	$: ({ stores } = data);

	import { onMount, onDestroy } from 'svelte';
	import { browser } from '$app/environment';
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import { HubConnectionBuilder } from '@microsoft/signalr';


	import { getToastStore } from '@skeletonlabs/skeleton';
	const toastStore = getToastStore();

	const baseUrl = 'https://localhost:6001';	

	let signalRConnectionState = 'Unknown';
	let connection;

	async function start() {
		if (!browser) return;

		connection = new HubConnectionBuilder()
			.withUrl(`${baseUrl}/hubs/testHub`)
			.withAutomaticReconnect()
			.build();

		connection.on('Notification', (message) => {
			console.log('Notification', message);
			const toast = { message: message, autohide: true, timeout: 1000};
			toastStore.trigger(toast);
		});

		// ("StoresStateChanged", store, currentCount, maxCapacity)
		connection.on('StoreStateChanged', (store, currentCount, maxCapacity) => {
			const toast = { message: `Store ${store} changed to ${currentCount}/${maxCapacity}`, autohide: true, timeout: 1000};
			toastStore.trigger(toast);

			stores.storeStates = stores.storeStates.map((s) => {
				if (s.store === store) {
					s.currentCount = currentCount;
					s.maxCapacity = maxCapacity;
				}
				return s;
			});
		});

		try {
			await connection.start();
			connection.send('Subscribe', 'storestates');
			signalRConnectionState = connection.state;
		} catch (err) {
			signalRConnectionState = connection.state;
			console.log('err', err);
			const toast = {
				message: `SignalR Error : ${err.message}`,
				background: 'variant-filled-error'
			};
			toastStore.trigger(toast);
			await start();
		}
	}
    
	onMount(async () => {
		await start();

		return () => {
			// console.log('onMount return function called');
			// connection.send('Unsubscribe', 'storestates');
			// connection.stop();
		};
	});

	onDestroy( () => {
		if (!browser) return;
			connection.send('Unsubscribe', 'storestates');
			connection.stop();
    });

</script>

<h1 class="h1 gradient-heading">Stores</h1>
<p>Current Total: {stores.totalVisitor} at stream {stores.revision}</p>

<div class="logo-cloud grid-cols-1 lg:!grid-cols-3 gap-1">
    {#each stores.storeStates as store}
        <a class="logo-item" href="/stores/store-{store.store}">
            <span><ProgressRadial value={100 * store.currentCount/store.maxCapacity}>{store.currentCount}/{store.maxCapacity}</ProgressRadial></span>
            <span>Store {store.store}</span>
        </a>
    {/each}
</div>
