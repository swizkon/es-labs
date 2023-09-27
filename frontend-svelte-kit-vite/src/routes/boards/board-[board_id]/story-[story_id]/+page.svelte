
<script>
    export let data;
    // const {products} = data;
    $: ( { product } = data )
</script>

<h1>{product.title}</h1>


<a href="/boards">Back</a>