<script>
	import { layoutContent } from '../lib/navStore';
	import { onMount, onDestroy } from 'svelte';
	import { browser } from '$app/environment';
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import { HubConnectionBuilder } from '@microsoft/signalr';

	import Level from '../components/Level.svelte';
	import Slider from '../lib/components/Slider.svelte';

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
			// console.log('Notification', message);
			const toast = { message: message, autohide: true, timeout: 1000};
			toastStore.trigger(toast);
			signalRMessageCount++;
		});

		connection.on('ZoneThresholdChanged', function (zone, threshold) {
			signalRMessageCount++;
			// console.log('ZoneThresholdChanged', zone, threshold);

			bikes = bikes.map((b) => {
				if (b[3] === zone) {
					b[1] = threshold;
				}
				return b;
			});
		});

		connection.on('ConfigChanged', function (config) {
			signalRMessageCount++;
			console.log('ConfigChanged', config);
		});

		try {
			await connection.start();
			connection.send('Subscribe', 'configs');
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

	async function stop() {
		if (!browser) return;
		connection.send('Unsubscribe', 'configs');
		await connection.stop();
	}

	onMount(async () => {
		if (!browser) return;
		layoutContent.set(['Configure zone capacity']);
		await start();

		return () => {
			connection.stop();
		};
	});

	onDestroy(async () => {
		layoutContent.set(['']);
		await stop();
    });

	let volume = 50;

	let levels = [
		{ zone: 'A', threshold: 25},
		{ zone: 'B', threshold: 25},
		{ zone: 'C', threshold: 25},
		{ zone: 'D', threshold: 25}];

	let bikes = [
		[[0, 0, -5], 25, 0x663399, 'A'],
		[[0, 0, 0], 25, 0x336699, 'B'],
		[[0, 0, 5], 25, 0x996633, 'C'],
		[[0, 0, 10], 25, 0x339966, 'D']
	];

	function handleZoneThresholdChanged(a, b) {
		connection.send('SetZoneThreshold', a, b);
	}

	function handleZoneThresholdChangedEvent(event) {
		// console.log('handleZoneThresholdChangedEvent', event.detail);
		var d = levels[event.detail.key];
		handleZoneThresholdChanged(`${d.zone}`, `${d.threshold}`);
	}
</script>

<SC.Canvas
	style="position:bottom;height:300px;"
	antialias
	background={new THREE.Color('#101010')}
	fog={new THREE.FogExp2('papayawhip', 0.01)}
	shadows
>
	<SC.Group position={[0, 0, 0]}>
		<SC.Primitive
			object={new THREE.GridHelper(40, 40, 'papayawhip', 'papayawhip')}
			position={[0, 0.1, 0]}
		/>
	</SC.Group>

	{#each bikes as b}
		<Level volume={volume} position={b[0]} level={b[1]} color={b[2]} />
	{/each}

	<SC.PerspectiveCamera position={[-40, 15, 30]} />
	<SC.OrbitControls autoRotate={false} enableZoom={true} maxPolarAngle={Math.PI * 0.51} />
	<SC.AmbientLight intensity={0.6} />
	<SC.DirectionalLight intensity={0.5} position={[-2, 3, 2]} shadow={{ mapSize: [2048, 2048] }} />
</SC.Canvas>

<div class="controls">
	{#each levels as level, i}
		<Slider
			on:sliderChange={handleZoneThresholdChangedEvent}
			label="Zone {level.zone}"
			key={i}
			bind:value={level.threshold}
		/>
	{/each}
	<div>
		<h2>ConnectionState: <small>{signalRConnectionState}</small></h2>
		<h2>Messages: <small>{signalRMessageCount}</small></h2>
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
