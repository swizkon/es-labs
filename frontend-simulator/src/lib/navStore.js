// layoutStore.js
import { writable } from 'svelte/store';

// Initialize the store with default content
export const layoutContent = writable('Default Layout Content');
export const isEmbedded = writable(false);
