<script>
	export let data;
	$: ({ product } = data);
</script>

<h1>{product.title}</h1>

<a href="/boards">Back</a>
