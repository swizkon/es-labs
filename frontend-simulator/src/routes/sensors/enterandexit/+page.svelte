<script>
	import { onMount, onDestroy } from 'svelte';
	import { browser } from '$app/environment';
	import { layoutContent } from '../../../lib/navStore';
	
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	export let data;
	$: ({ store, zones } = data);

	import { HubConnectionBuilder } from '@microsoft/signalr';
	
	import StoreZone from '../../../components/StoreZone.svelte';

	import { getToastStore } from '@skeletonlabs/skeleton';
	const toastStore = getToastStore();

	const hubUrl = 'https://localhost:4001/hubs/messageExchange';

	let turnstiles = [
		{ title: 'Entry door', id: '0A', position: [0, 0, 0]},
		{ title: 'Exit door', id: 'D0', position: [0, 0, 0]}
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
		layoutContent.set(['Store 1: Enter / exit']);
	});

	onDestroy( () => {
		layoutContent.set(['Retail Rhythm Radar - Simulator']);
		if (!browser) return;
			connection.send('Unsubscribe', `store-1-states`);
			connection.stop();
    });

	function sendSignal(store, turnstile, direction) {
		fetch(`http://localhost:4000/events/TurnstilePassageDetected`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify({
			"direction": direction,
			"turnstile": {
				"SerialNumber": `1-${turnstile}`
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
	fetch(`http://localhost:4000/commands/ResetZone`, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json'
		},
		body: JSON.stringify({
		"store": store,
		"zone": zone,
		"who": "Malloc Frobnitz",
		"reason": "No one there..."
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
		<StoreZone zone={zone.zone} visitors={zone.visitors} position={zone.position} color={zone.color} size={zone.size} />
	{/each}

	<SC.PerspectiveCamera position={[-40, 15, 30]} />
	<SC.OrbitControls enableZoom={true} maxPolarAngle={Math.PI * 0.51} />
	<SC.AmbientLight intensity={0.6} />
	<SC.DirectionalLight intensity={0.5} position={[-2, 3, 2]} shadow={{ mapSize: [2048, 2048] }} />
</SC.Canvas>

<div class="controls">
	{#each turnstiles as turnstile}
		{turnstile.title} <br/>
		<button title="Move from RIGHT to LEFT" class="btn variant-filled-primary" on:click={() => sendSignal(store, turnstile.id, 'counterClockwise')}>&#8630;</button>
		<button title="Move from LEFT to RIGHT" class="btn variant-filled-primary" on:click={() => sendSignal(store, turnstile.id, 'clockwise')}>&#8631;</button>
		<hr	/>
	{/each}
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
