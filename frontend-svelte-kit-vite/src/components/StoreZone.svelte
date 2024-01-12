<script>
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	export let position = [0, 0, 0];
	export let size = [10, 10];

	export let color = 0x003eff;
	export let visitors = 10;
</script>

<SC.Group scale={[0.1, 0.1, 0.1]} {position}>
	<SC.Mesh
		geometry={new THREE.BoxGeometry(size[0], visitors, size[1])}
		material={new THREE.MeshStandardMaterial({ color: color })}
		scale={[1, 1, 1]}
		position={[1, visitors * 0.5, 0]}
		castShadow
	/>
</SC.Group>
