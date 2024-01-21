<script>
    import { ProgressRadial } from '@skeletonlabs/skeleton';

	import { onMount, onDestroy } from 'svelte';
	import { browser } from '$app/environment';
	import { layoutContent, isEmbedded } from '../../../lib/navStore';
	
	export let data;
	$: ({ averageTimeProjection, date } = data);

	import { HubConnectionBuilder } from '@microsoft/signalr';

	import { getToastStore } from '@skeletonlabs/skeleton';
	const toastStore = getToastStore();

	const hubUrl = 'https://localhost:4001/hubs/messageExchange';

	let signalRConnectionState = 'Unknown';
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
			// signalRMessageCount++;
		});
		
		connection.on('AverageTimeProjectionChanged', (store, message) => {
			console.log('AverageTimeProjectionChanged', message);
			averageTimeProjection = message;
		});

		// ("StoresStateChanged", store, currentCount, maxCapacity)
		connection.on('StoreStateChanged', (store, currentCount, maxCapacity) => {
			stores.storeStates = stores.storeStates.map((s) => {
				if (s.store === store) {
					s.currentCount = currentCount;
					s.maxCapacity = maxCapacity;
				}
				return s;
			});

			stores.totalVisitor = stores.storeStates.reduce((acc, s) => acc + s.currentCount, 0);
		});

		connection.on('ZoneStateChanged', (store, zone, currentCount, maxCapacity) => {
			zones = zones.map((z) => {
				if (z.zone === zone) {
					z.visitors = currentCount;
				}
				return z;
			});
		});

		try {
			await connection.start();
			connection.send('Subscribe', `store-1-states`);
			connection.send('Subscribe', `storestates`);
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
		// isEmbedded.set(true);
		layoutContent.set(['Streaming projection: avg time per visitor']);
	});

	onDestroy( () => {
		layoutContent.set(['']);
		isEmbedded.set(false);
		if (!browser) return;
			connection.send('Unsubscribe', `store-1-states`);
			connection.stop();
    });


</script>

Store 1
<div class="logo-cloud grid-cols-1 lg:!grid-cols-3 gap-1">

	<div class="logo-item">
		<span><ProgressRadial value={averageTimeProjection.currentNumberOfVisitors}>{averageTimeProjection.currentNumberOfVisitors}</ProgressRadial></span>
		<span>Currently {averageTimeProjection.currentNumberOfVisitors} with average time {averageTimeProjection.averageTime}</span>
	</div>
	
	<div class="logo-item">
		<span><ProgressRadial value={averageTimeProjection.totalOfVisitors}>{averageTimeProjection.totalOfVisitors}</ProgressRadial></span>
		<span>Total {averageTimeProjection.totalOfVisitors} with totalTime {averageTimeProjection.totalTime}</span>
	</div>
</div>
