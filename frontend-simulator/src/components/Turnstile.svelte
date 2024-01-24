<script>
	import * as THREE from 'three';
	import { FontLoader } from 'three/examples/jsm/loaders/FontLoader.js';
	import { TextGeometry } from 'three/examples/jsm/geometries/TextGeometry.js';
	import * as SC from 'svelte-cubed';

	export let position = [0, 0, 0];

	export let color = 0x666666;
	export let id;

	let defaultFont;
	let textGeometry;

	let loader = new FontLoader();
	loader.load('/fonts/Permanent_Marker_Regular.json', function (font) {
		defaultFont = font;
		textGeometry = new TextGeometry(`${id}`, {
			font: font,
			size: 14,
			height: 12,
			curveSegments: 12,
			bevelThickness: 1,
			bevelSize: 1,
			bevelEnabled: false
		});
	});

</script>

<SC.Group scale={[0.1, 0.1, 0.1]} {position}>
	
	<SC.Mesh
		geometry={new THREE.CylinderGeometry(2,2,60)}
		material={new THREE.MeshStandardMaterial({ color: color })}
		scale={[1, 1, 1]}
		position={[10, 30, 10]}
		castShadow
	/>

	{#if textGeometry}
	<SC.Mesh
		geometry={textGeometry}
		material={new THREE.MeshStandardMaterial({ color: color })}
		scale={[1, 1, 1]}
		position={[-5, 60, 0]}
		castShadow
	/>
	{/if}
</SC.Group>
