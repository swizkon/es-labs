<script>
	import { onMount } from 'svelte';
	import { browser } from '$app/env';
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import { HubConnectionBuilder } from '@microsoft/signalr';

	import Logo from '../components/Logo.svelte';
	import Level from '../components/Level.svelte';

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

		try {
			await connection.start();
			connection.send('Broadcast', 'HalfSpecial', 'Just connected');
			signalRConnectionState = connection.state;
			console.log('SignalR Connected.');
		} catch (err) {
			signalRConnectionState = connection.state;
			console.log('err', err);
		}
	}

	onMount(async () => {
		await start();
	});

	let volume = 0.1;

	function handleLevelChanged(a, b) {
		connection.send('SetChannelLevel', roomName, '' + a, b);
	}

	let levels = [0];
	let bikes = [[[0, 0, 10], 50, 0x669933]];
</script>

<SC.Canvas
	antialias
	background={new THREE.Color('black')}
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
		<Level position={b[0]} level={b[1]} color={b[2]} />
	{/each}

	<SC.PerspectiveCamera position={[-40, 15, 30]} />
	<SC.OrbitControls enableZoom={true} maxPolarAngle={Math.PI * 0.51} />
	<SC.AmbientLight intensity={0.6} />
	<SC.DirectionalLight
		intensity={0.5}
		position={[-2, 3, 2]}
		shadow={{ mapSize: [2048, 2048] }}
	/>
</SC.Canvas>

<Logo />

<div class="controls">
	{#each levels as level, i}
		<label
			><input
				type="range"
				on:input={(e) => handleLevelChanged(i, e.target.value)}
				bind:value={levels[i]}
				min={1}
				max={100}
				step={1}
			/>
			level{i}
		</label>
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
		top: 1em;
		width: 500;
		color: antiquewhite;
	}

	label {
		display: flex;
		width: 60px;
		gap: 0.5em;
		align-items: center;
	}

	input {
		width: 200px;
		margin: 0;
	}
</style>
