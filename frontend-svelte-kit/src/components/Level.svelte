<script>
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	export let position = [0, 0, 0];
	export let color = 0x003eff;
	export let level = 50;
</script>

<SC.Group scale={[0.1, 0.1, 0.1]} position={position}>
	<SC.Mesh
		geometry={new THREE.BoxGeometry(45, level, 45)}
		material={new THREE.MeshStandardMaterial({ color: color })}
		scale={[1, 1, 1]}
		position={[1.6, level - level/2.45, 0]}
		castShadow
	/>
</SC.Group>
