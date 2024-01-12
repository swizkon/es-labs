<script>
	import { onMount, onDestroy } from 'svelte';
	import { layoutContent } from '../../../lib/navStore';
	
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	import Link from '../../../components/base/Link.svelte';

	export let data;
	$: ({ product, title } = data);

	onMount(() => {
		layoutContent.set([title]);
	});
	
	onDestroy(() => {
		layoutContent.set([]);
	});
</script>

<h1 class="h1 gradient-heading">{title}</h1>

<hr />


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



<hr />
<Link href="/stores">Back</Link>
