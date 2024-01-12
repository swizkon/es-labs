<script>
	
	import { onMount, onDestroy } from 'svelte';

	import { layoutContent } from '../../../lib/navStore';

	import Link from '../../../components/base/Link.svelte';

	export let data;
	$: ({ product } = data);

	onMount(() => {
		layoutContent.set(['/boards/board-2', '/boards/board-1']);
	});
</script>

<h1 class="h1 gradient-heading">{product.title}</h1>

{#each product.images as image}
	<h2 class="h2">{image}</h2>
	<a href="/boards/board-{product.id}/story-{product.id}">Show</a>
{/each}

<hr />
<Link href="/boards">Back</Link>
