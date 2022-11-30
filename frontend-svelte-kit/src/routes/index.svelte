<script>
	import { onMount } from 'svelte';
	import { browser } from '$app/env';
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

	import Logo from '../components/Logo.svelte';
	import Level from '../components/Level.svelte';

	import { of } from 'rxjs';
	import { delay, startWith } from 'rxjs/operators';

	const baseUrl = 'https://localhost:6001';

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
		
		connection.on('ChannelLevel', (player, x, y) => {
			console.log('ChannelLevel', player, x, y);
			bikes[x][0][1] = -y/100;
			bikes[x][1] = y;
			signalRMessageCount++;
		});
		
		connection.on('VolumeChanged', (deviceName, volume) => {
			console.log('VolumeChanged', deviceName, volume);
			// bikes[x][0][1] = -y/100;
			// bikes[x][1] = y;
			signalRMessageCount++;
		});

		try {
			await connection.start();
			connection.send('Broadcast', 'Me', 'Just connected');
			signalRConnectionState = connection.state;
			console.log('SignalR Connected.');
		} catch (err) {
			signalRConnectionState = connection.state;
			console.log('err', err);
		}
		console.log(connection);
	}

	onMount(async () => {
		await start();
	});

	// emit an array with initial delay of 2s
	const values = of([10]).pipe(delay(2000), startWith([]));

	let width = 1;
	let height = 0.1;

	$: onChange(width, height);

	function onChange(...args) {
		console.log(args)
	}

	function handleLevelChanged(a, b) {
		connection.send('SetChannelLevel', 'mainroom', '' + a, b);
	}

	function handleVolumeChanged(v) {
		connection.send('SetVolume', 'mainroom', v);
	}

	let levels = [
		50,
		50,
		50,
		50,
		50
	];
	let bikes = [
		[[0, 0, -10], 50, 0x663399],
		[[0, 0, -5],  50, 0x336699],
		[[0, 0,  0],  50, 0x996633],
		[[0, 0,  5],  50, 0x339966],
		[[0, 0, 10],  50, 0x669933]
	];
</script>

<SC.Canvas
	antialias
	background={new THREE.Color('black')}
	fog={new THREE.FogExp2('papayawhip', 0.01)}
	shadows
>
	<SC.Group position={[0, 0, 0]}>
		<SC.Mesh
			geometry={new THREE.PlaneGeometry(20, 40)}
			material={new THREE.MeshStandardMaterial({ color: 'burlywood' })}
			rotation={[-Math.PI / 2, 0, 0]}
			receiveShadow
		/>
		<SC.Primitive
			object={new THREE.GridHelper(40, 40, 'papayawhip2', 'papayawhip2')}
			position={[0, 0.001, 0]}
		/>
	</SC.Group>

	
	{#each bikes as b}
		<Level position={b[0]} color={b[2]} level={b[1]} />
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
		><input type="range" on:input={(e) => handleLevelChanged(i, e.target.value)} bind:value={levels[i]} min={1} max={100} step={1} />
		level{i}
		</label
	>
	{/each}

	<label
		><input type="range" on:input={(e) => handleVolumeChanged(e.target.value)} bind:value={height} min={0} max={100} step={1} /> volume</label
	>
	<div>
		<h2>State: <small>{signalRConnectionState}</small></h2>
		<h2>Messages: <small>{signalRMessageCount}</small></h2>
	</div>
</div>

<style>
	.controls {
		position: absolute;
		left: 1em;
		top: 1em;
		width:500;
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
