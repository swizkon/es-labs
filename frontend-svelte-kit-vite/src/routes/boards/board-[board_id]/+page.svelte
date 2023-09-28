<script>
	export let data;
	$: ({ product } = data);
</script>

<h1 class="h1 gradient-heading">{product.title}</h1>

{#each product.images as image}
	<h2 class="h2">{image}</h2>
	<a href="/boards/board-{product.id}/story-{product.id}">Show</a>
{/each}

<hr />
<a href="/boards">Back</a>
