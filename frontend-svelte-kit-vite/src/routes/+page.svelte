<script>
	import { onMount } from 'svelte';
	import { browser } from '$app/environment';
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import { HubConnectionBuilder } from '@microsoft/signalr';

	import Logo from '../components/Logo.svelte';
	import Level from '../components/Level.svelte';
	import Slider from '../lib/components/Slider.svelte';

	import { getToastStore } from '@skeletonlabs/skeleton';
	const toastStore = getToastStore();

	const baseUrl = 'https://localhost:6001';
	const roomName = 'mainroom';

	let signalRConnectionState = 'Unknown';
	let signalRMessageCount = 0;
	let connection;

	async function start() {
		console.log(browser);
		if (!browser) return;

		connection = new HubConnectionBuilder()
			.withUrl(`${baseUrl}/hubs/testHub`)
			.withAutomaticReconnect()
			.build();

		connection.on('EqualizerStateChanged', function (data) {
			console.log('EqualizerStateChanged', data);

			for (let index = 0; index < data.channels.length; index++) {
				const element = data.channels[index];
				const pos = parseInt(element.channel);
				const lev = parseInt(element.level);
				levels[pos] = lev;
				bikes[pos][1] = lev;
			}
			volume = data.volume;
		});

		connection.on('ChannelLevel', (player, x, y) => {
			console.log('ChannelLevel', player, x, y);
			bikes[x][1] = y;
			signalRMessageCount++;
		});

		connection.on('VolumeChanged', (deviceName, v) => {
			console.log('VolumeChanged', deviceName, v);
			projection.volume = parseInt(v);
			signalRMessageCount++;
		});

		try {
			await connection.start();
			connection.send('Broadcast', 'index.svelte', 'Just connected');
			signalRConnectionState = connection.state;
			console.log('SignalR Connected.');
			const toast = { message: 'SignalR Connected.' };
			toastStore.trigger(toast);
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
	});

	let volume = 0.1;

	let levels = [0, 0, 0, 0, 0];

	let projection = {
		volume: 0.1
	};
	let bikes = [
		[[0, 0, -10], 50, 0x663399],
		[[0, 0, -5], 50, 0x336699],
		[[0, 0, 0], 50, 0x996633],
		[[0, 0, 5], 50, 0x339966],
		[[0, 0, 10], 50, 0x669933]
	];

	function handleLevelChanged(a, b) {
		connection.send('SetChannelLevel', roomName, '' + a, b);
	}

	function handleLevelChangedEvent(event) {
		console.log('page.svelte:handleLevelChangedEvent', event.detail);
		handleLevelChanged('' + event.detail.key, '' + event.detail.value);
	}

	function handleVolumeChanged(v) {
		console.log('handleVolumeChanged', 'v', v, typeof v);
		connection.send('SetVolume', roomName, v);
	}

	function handleVolumeChangedEvent(event) {
		console.log('page.svelte:handleVolumeChangedEvent', event.detail);
		handleVolumeChanged('' + event.detail.value);
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
			object={new THREE.GridHelper(40, 40, 'papayawhip2', 'papayawhip2')}
			position={[0, 0.1, 0]}
		/>
	</SC.Group>

	{#each bikes as b}
		<Level volume={projection.volume} position={b[0]} level={b[1]} color={b[2]} />
	{/each}

	<SC.PerspectiveCamera position={[-40, 15, 30]} />
	<SC.OrbitControls enableZoom={true} maxPolarAngle={Math.PI * 0.51} />
	<SC.AmbientLight intensity={0.6} />
	<SC.DirectionalLight intensity={0.5} position={[-2, 3, 2]} shadow={{ mapSize: [2048, 2048] }} />
</SC.Canvas>

<Logo />

<div class="controls">
	{#each levels as level, i}
		<Slider
			on:sliderChange={handleLevelChangedEvent}
			label="Level {i}"
			key={i}
			bind:value={level}
		/>
	{/each}

	<Slider
		on:sliderChange={handleVolumeChangedEvent}
		label="Volume II"
		key="volume"
		bind:value={volume}
	/>
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
