<script>
	import { onMount } from 'svelte';
	import { browser } from '$app/env';
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

	import Bike from '../components/bike/Bike.svelte';

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

		connection.on('AddItem', (a,b,c) => {
			console.log(a)
			console.log(b)
			console.log(c)
			signalRMessageCount++;
		});
		connection.on('playerPosition', (player, x, y) => {
			bikes[0] = [x/10,0,y/10];
			signalRMessageCount++;
		});

		try {
			await connection.start();
			connection.send('Broadcast', 'user', 'message');
			signalRConnectionState = connection.state;
			console.log('SignalR Connected.');
		} catch (err) {
			signalRConnectionState = connection.state;
			console.log('err', err);
			// setTimeout(() => start(), 5000);
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
	let depth = 1;

	$: onChange(width, height, depth);

	function onChange(...args) {
		console.log(args)
	}

	function handleLevelChanged(a, b) {
		console.log(a, b)
		// connection.send('Broadcast', 'user', 'message');
	}

	let levels = [
		0,
		0,
		0,
		0,
		0,
		0,
		0,
	];
	let bikes = [
		[0, 0, -10],
		[0, 1, 0]
	];
</script>

<SC.Canvas
	antialias
	background={new THREE.Color('papayawhip')}
	fog={new THREE.FogExp2('papayawhip', 0.01)}
	shadows
>
	<SC.Group position={[0, -height / 2, 0]}>
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
		<Bike position={b} />
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

<div class="controls">

	{#each levels as level, i}
	<label
		><input type="range" on:input={(e) => handleLevelChanged(i, e.target.value)} bind:value={levels[i]} min={0.1} max={10} step={0.1} />
		level {i}
		</label
	>
	{/each}

	<label
		><input type="range" bind:value={height} min={0} max={10} step={1} /> volume</label
	>
	<label
		><input type="range" bind:value={depth} min={0.1} max={3} step={0.1} /> depth</label
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
