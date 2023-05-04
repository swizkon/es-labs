<script>
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	export let position = [0, 0, 0];
	export let color = 0x003eff;
	export let level = 50;
	export let volume = 50;
</script>

<SC.Group scale={[0.1, 0.1, 0.1]} position={position}>
	<SC.Mesh
		geometry={new THREE.BoxGeometry(volume, level, 45)}
		material={new THREE.MeshStandardMaterial({ color: color })}
		scale={[1, 1, 1]}
		position={[1, level * 0.5, 0]}
		castShadow
	/>
</SC.Group>
