<script>
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';
	import BikeWheel from './BikeWheel.svelte';

	export let position = [0, 0, 0];
</script>

<SC.Group scale={[0.5, 0.5, 0.5]} position={position}>
	<SC.Mesh
		geometry={new THREE.BoxGeometry(50, 5, 10)}
		material={new THREE.MeshStandardMaterial({ color: 0x0099ff })}
		scale={[0.1, 0.1, 0.1]}
		position={[4, 1.6, 0]}
		castShadow
	/>
	
	<SC.Mesh
		geometry={new THREE.BoxGeometry(3, 50, 5)}
		material={new THREE.MeshStandardMaterial({ color: 0x0099ff })}
		scale={[0.1, 0.1, 0.1]}
		position={[1.6, 3.9, 0]}
		castShadow
	/>
	
	<SC.Mesh
		geometry={new THREE.BoxGeometry(3, 3, 30)}
		material={new THREE.MeshStandardMaterial({ color: 0x003eff })}
		scale={[0.1, .1, 0.1]}
		position={[1.6, 6.3, 0]}
		castShadow
	/>

	<BikeWheel position={[8, 1.3, 0]} />

	<BikeWheel position={[0, 1.3, 0]} />
</SC.Group>
