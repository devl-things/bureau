import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [react()],
});

//export default defineConfig({
//    plugins: [react()],
//    base: '/react-spa-app/', // Base path for React app
//    build: {
//        outDir: '../Bureau.UI.Web/wwwroot/react-spa-app', // Output to ASP.NET's wwwroot
//        emptyOutDir: true, // Clean the output directory before building
//    },
//});