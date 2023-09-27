
<script>
    export let data;
    $: ( { products } = data )
</script>

<h1>The boards page</h1>
{#each products as product}
    <h2>{product.title}</h2>
    <a href="/boards/board-{product.id}">Show</a>
{/each}