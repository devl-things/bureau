import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";

const repoRoot = path.resolve(__dirname, "../..");

export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            // serve workspace libs from source in dev
            "@bureau/client-core": path.resolve(repoRoot, "libs/client-core/src"),
            "@bureau/chores-admin": path.resolve(repoRoot, "libs/chores-admin/src"),
        },
        // helps avoid duplicate react when using workspaces
        dedupe: ["react", "react-dom"],
    },
    server: {
        port: 5001,
        strictPort: true,
        fs: {
            allow: [repoRoot],
        },
    },
    build: {
        outDir: "dist",
        emptyOutDir: true,
        sourcemap: true,
        rollupOptions: {
            input: {
                // stable entry name
                "chores-admin": path.resolve(__dirname, "src/entries/chores-admin.ts")
            },
            output: {
                // final output paths inside dist
                entryFileNames: "admin/assets/[name].js",
                chunkFileNames: "admin/assets/chunks/[name]-[hash].js",
                assetFileNames: "admin/assets/[name]-[hash][extname]"
            }
        }
    }
});
