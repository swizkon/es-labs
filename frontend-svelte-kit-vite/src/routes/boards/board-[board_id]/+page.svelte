
<script>
    export let data;
    $: ( { product } = data )
</script>

<h1>{product.title}</h1>


<h1>The boards page</h1>
{#each product.images as image}
    <h2>{product.title}</h2>
    <a href="/boards/board-{product.id}/story-{product.id}">Show</a>
{/each}

<hr/>
<a href="/boards">Back</a>