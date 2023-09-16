/** @type {import('@sveltejs/kit').Config} */

import adapter from '@sveltejs/adapter-static';

const config = {
  kit: {
    // hydrate the <div id="svelte"> element in src/app.html
    target: '#svelte',

    vite: {
      ssr: {
        noExternal: ['three', 'fluent-svelte'],
      },
    },
		adapter: adapter({
			// default options are shown. On some platforms
			// these options are set automatically — see below
			// pages: 'buildzzz',
			// assets: 'buildzzz',
			// fallback: undefined,
      
			fallback: '200.html',
			precompress: false,
			strict: true
		})
  },
};

export default config;
