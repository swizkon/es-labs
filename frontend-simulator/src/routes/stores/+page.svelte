<script>
	import { layoutContent } from '../../lib/navStore';

    import { ProgressRadial } from '@skeletonlabs/skeleton';
	export let data;
	$: ({ stores, date } = data);

	import { onMount, onDestroy } from 'svelte';
	import { browser } from '$app/environment';

	import { HubConnectionBuilder } from '@microsoft/signalr';


	import { getToastStore } from '@skeletonlabs/skeleton';
	const toastStore = getToastStore();

	const hubUrl = 'https://localhost:4001/hubs/messageExchange';

	let signalRConnectionState = 'Unknown';
	let signalRMessageCount = 0;
	let connection;

	async function start() {
		if (!browser) return;

		connection = new HubConnectionBuilder()
			.withUrl(hubUrl)
			.withAutomaticReconnect()
			.build();

		connection.on('Notification', (message) => {
			console.log('Notification', message);
			const toast = { message: message, autohide: true, timeout: 1000};
			toastStore.trigger(toast);
			signalRMessageCount++;
		});

		connection.on('StoreStateChanged', (store, currentCount, maxCapacity) => {
			stores.storeStates = stores.storeStates.map((s) => {
				if (s.store === store) {
					s.currentCount = currentCount;
					s.maxCapacity = maxCapacity;
				}
				return s;
			});

			stores.totalVisitor = stores.storeStates.reduce((acc, s) => acc + s.currentCount, 0);
			signalRMessageCount++;
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
		layoutContent.set(['Stores']);
		await start();

		return () => {
		};
	});

	onDestroy( () => {
		layoutContent.set(['']);
		if (!browser) return;
			connection.send('Unsubscribe', 'storestates');
			connection.stop();
    });

</script>

<!-- <h1 class="h1 gradient-heading">Stores</h1> -->
<p>Current Total: {stores.totalVisitor} at stream &#8470; {stores.revision}</p>

<div>
	<h2>ConnectionState: <small>{signalRConnectionState}</small></h2>
	<h2>Messages: <small>{signalRMessageCount}</small></h2>
</div>

<div class="logo-cloud grid-cols-1 lg:!grid-cols-3 gap-1">
    {#each stores.storeStates as store}
        <a class="logo-item" href="/stores/store-{store.store}-date-{date}">
            <span><ProgressRadial value={100 * store.currentCount/store.maxCapacity}>{store.currentCount}/{store.maxCapacity}</ProgressRadial></span>
            <span>Store {store.store}</span>
        </a>
    {/each}
</div>
