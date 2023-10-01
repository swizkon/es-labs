<script>
	import Link from '../../components/base/Link.svelte';
	export let data;
	$: ({ products } = data);
</script>

<h1 class="h1 gradient-heading">The boards page</h1>
{#each products as product}
	<h2>{product.title}</h2>
	<Link href="/boards/board-{product.id}">Show</Link>
{/each}
