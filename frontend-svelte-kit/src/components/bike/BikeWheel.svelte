<script>
	import * as THREE from 'three';
	import * as SC from 'svelte-cubed';

	export let position;
</script>

<SC.Mesh
	geometry={new THREE.TorusGeometry(10, 4, 10, 100, 7)}
	material={new THREE.MeshStandardMaterial({ color: 0x333333 })}
	scale={[0.1, 0.1, 0.1]}
	rotation={[0, 0, 0]}
	{position}
	castShadow
/>
