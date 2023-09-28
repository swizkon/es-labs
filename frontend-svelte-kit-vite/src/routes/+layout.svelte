<script>
	import '../app.postcss';
</script>

<nav>
	<a href="/">Home</a>
	<a href="/about">about</a>
	<a href="/boards">boards</a>
</nav>

<div class="container h-full mx-auto flex justify-center items-center">
	<div class="space-y-5">
		<slot />
	</div>
</div>
