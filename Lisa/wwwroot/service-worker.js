const cacheName = 'blazor-server-cache-v1';
const assetsToCache = [
    '/', // Root
    '/manifest.json', // Manifest file
    '/icon-192x192.png', // Icon files
    '/icon-512x512.png',
    '/css/site.css', // Ensure this file exists
    '/favicon.png', // Favicon
    '/offline.html',
    '_framework/blazor.server.js'
];

// Install event - Create the cache and add assets
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(cacheName).then(async cache => {
            console.log(`Opened cache: ${cacheName}`);
            for (const asset of assetsToCache) {
                try {
                    const response = await fetch(asset);
                    if (response.ok) {
                        await cache.put(asset, response.clone());
                        console.log(`Cached: ${asset}`);
                    } else {
                        console.warn(`Failed to fetch ${asset}: ${response.statusText}`);
                    }
                } catch (error) {
                    console.error(`Failed to cache ${asset}:`, error);
                }
            }
        })
    );
});

// Activate event - Clean up old caches
self.addEventListener('activate', event => {
    const cacheWhitelist = [cacheName]; // Only keep the current cache
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.map(key => {
                    if (!cacheWhitelist.includes(key)) {
                        console.log(`Deleting old cache: ${key}`);
                        return caches.delete(key);
                    }
                })
            )
        )
    );
});


// Fetch event - Serve cached files when offline
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Skip requests to the Blazor framework directory
    if (url.pathname.startsWith('/_framework/')) {
        return;
    }

    event.respondWith(
        fetch(event.request).catch(() =>
            caches.match(event.request).then(response => response || caches.match('/offline.html'))
        )
    );
});