<script>
	import * as THREE from 'three';
	import { FontLoader } from 'three/examples/jsm/loaders/FontLoader.js';
	import { TextGeometry } from 'three/examples/jsm/geometries/TextGeometry.js';
	import * as SC from 'svelte-cubed';

	export let zone = "Zone";
	export let position = [0, 0, 0];
	export let size = [10, 10];

	export let color = 0x003eff;
	export let visitors = 10;

	let defaultFont;
	let textGeometry;

	$: visitors > -888 && myfunc();

	function myfunc() {
		textGeometry = new TextGeometry(`${zone}:${visitors}`, {
			font: defaultFont,
			size: 36,
			height: 12,
			curveSegments: 12,
			bevelThickness: 1,
			bevelSize: 1,
			bevelEnabled: false
		});
	}

	let loader = new FontLoader();
	loader.load('/fonts/Permanent_Marker_Regular.json', function (font) {
		defaultFont = font;
		textGeometry = new TextGeometry(`${zone}:${visitors}`, {
			font: font,
			size: 36,
			height: 10,
			curveSegments: 12,
			bevelThickness: 0.5,
			bevelSize: 1,
			bevelEnabled: false
		});
	});

</script>

<SC.Group scale={[0.1, 0.1, 0.1]} {position}>
	<SC.Mesh
		geometry={new THREE.BoxGeometry(size[0], visitors, size[1])}
		material={new THREE.MeshStandardMaterial({ color: color })}
		scale={[1, 1, 1]}
		position={[1, visitors * 0.5, 0]}
		castShadow
	/>
	{#if textGeometry}
	<SC.Mesh
		geometry={textGeometry}
		material={new THREE.MeshStandardMaterial({ color: color, roughness: 0.1})}
		scale={[1, 1, 1]}
		position={[10, visitors, -30]}
		rotation={[0, - Math.PI / 2, 0]}
		castShadow
	/>
	{/if}
</SC.Group>
