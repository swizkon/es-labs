import Main from './Main.svelte';

const app = new Main({
    target: document.body,
    props: {
        name: 'buyscout 1.0'
    }
});

export default app;