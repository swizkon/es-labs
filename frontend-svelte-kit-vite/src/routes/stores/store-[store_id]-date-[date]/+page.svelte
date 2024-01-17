<script>
	import { onMount, onDestroy } from 'svelte';
	import { browser } from '$app/environment';
	import { layoutContent } from '../../../lib/navStore';
	
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import Link from '../../../components/base/Link.svelte';

	export let data;
	$: ({ store, title, zones } = data);

	import { HubConnectionBuilder } from '@microsoft/signalr';
	
	import StoreZone from '../../../components/StoreZone.svelte';

	import { getToastStore } from '@skeletonlabs/skeleton';
	const toastStore = getToastStore();

	const hubUrl = 'https://localhost:4001/hubs/messageExchange';

	let turnstiles = [
		{ id: '0A', position: [0, 0, 0]},
		{ id: 'AB', position: [0, 0, 0]},
		{ id: 'AC', position: [0, 0, 0]},
		{ id: 'BC', position: [0, 0, 0]},
		{ id: 'BD', position: [0, 0, 0]},
		{ id: 'CD', position: [0, 0, 0]},
		{ id: 'D0', position: [0, 0, 0]}
	];

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
		});

		connection.on('ZoneStateChanged', (store, zone, currentCount, maxCapacity) => {
			// const toast = { message: `Store ${store} zone ${zone} changed to ${currentCount}/${maxCapacity}`, autohide: true, timeout: 1000};
			// toastStore.trigger(toast);

			zones = zones.map((z) => {
				if (z.zone === zone) {
					z.visitors = currentCount;
				}
				return z;
			});
		});

		try {
			await connection.start();
			connection.send('Subscribe', `store-${store}-states`);
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
		layoutContent.set([title]);

		return () => {
			// console.log('onMount return function called');
			// connection.send('Unsubscribe', 'storestates');
			// connection.stop();
		};
	});

	onDestroy( () => {
		layoutContent.set([]);
		if (!browser) return;
			connection.send('Unsubscribe', `store-${store}-states`);
			connection.stop();
    });

	function sendSignal(store, turnstile, direction) {
		fetch(`http://localhost:4000/StoreFlow/events/TurnstilePassageDetected`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify({
			"direction": direction,
			"turnstile": {
				"SerialNumber": `${store}-${turnstile}`
			}
			})
		})
			.then((response) => {
				// console.log('response', response);
			})
			.catch((err) => {
				console.log('err', err);
				const toast = { message: `Turnstile ${turnstile} increment failed`, autohide: true, timeout: 1000};
				toastStore.trigger(toast);
			});
	}

function sendResetCommand(store, zone) {
	fetch(`http://localhost:4000/StoreFlow/commands/ResetZone`, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json'
		},
		body: JSON.stringify({
		"store": store,
		"zone": zone
		})
	})
		.then((response) => {
			console.log('response', response);
		})
		.catch((err) => {
			console.log('err', err);
			const toast = { message: `Reset zone ${zone} failed`, autohide: true, timeout: 1000};
			toastStore.trigger(toast);
		});
}

</script>


<SC.Canvas
	style="position:bottom;height:300px;"
	antialias
	background={new THREE.Color('#999999')}
	fog={new THREE.FogExp2('papayawhip', 0.01)}	
	shadows
>
	<SC.Group position={[0, 0, 0]}>
		<SC.Primitive
			object={new THREE.GridHelper(30, 30, 'papayawhip2', 'papayawhip2')}
			position={[0, -0.1, 0]}
		/>
	</SC.Group>

	{#each zones as zone}
		<StoreZone visitors={zone.visitors} position={zone.position} color={zone.color} size={zone.size} />
	{/each}

	<SC.PerspectiveCamera position={[-40, 15, 30]} />
	<SC.OrbitControls enableZoom={true} maxPolarAngle={Math.PI * 0.51} />
	<SC.AmbientLight intensity={0.6} />
	<SC.DirectionalLight intensity={0.5} position={[-2, 3, 2]} shadow={{ mapSize: [2048, 2048] }} />
</SC.Canvas>

<div class="controls">
	{#each turnstiles as turnstile}
		{turnstile.id}
		<button title="Move from RIGHT to LEFT" class="btn variant-filled-primary" on:click={() => sendSignal(store, turnstile.id, 'counterClockwise')}>&#8630;</button>
		<button title="Move from LEFT to RIGHT" class="btn variant-filled-primary" on:click={() => sendSignal(store, turnstile.id, 'clockwise')}>&#8631;</button>
		<hr	/>
	{/each}
	<hr />
	<div>
	{#each zones as z}
		<button title="Reset zone {z.zone} to zero" class="btn variant-filled-primary" on:click={() => sendResetCommand(store, z.zone)}>&#9850; {z.zone}</button>&nbsp;
	{/each}
	</div>
	<hr />
	<div>
		<h2>ConnectionState: <small>{signalRConnectionState}</small></h2>
	</div>
</div>

<style>
	.controls {
		position: absolute;
		left: 1em;
		bottom: 1em;
		width: 700;
		color: antiquewhite;
	}
</style>
