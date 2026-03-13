import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const libs = (name: string) =>
    path.resolve(__dirname, `../../libs/${name}/src/index.ts`);
const shared = (name: string) =>
    path.resolve(__dirname, `../../../shared/${name}/src/index.ts`);

export default defineConfig({
    plugins: [react()],
    server: {
        port: 5000,
        strictPort: true,
    },
    resolve: {
        dedupe: ["react", "react-dom", "react-router-dom"],
        alias: {
            "@bureau/bureau-shell": libs("bureau-shell"),
            "@bureau/client-core": libs("client-core"),
            "@bureau/chores-admin": libs("chores-admin"),
            "@bureau/chores-core": shared("chores-core"),
            "@bureau/nodes-admin": libs("nodes-admin"),
            "@bureau/nodes-core": shared("nodes-core"),
            "@bureau/offline-store": shared("offline-store"),
        },
    },
    optimizeDeps: {
        exclude: [
            "@bureau/bureau-shell",
            "@bureau/client-core",
            "@bureau/chores-admin",
            "@bureau/chores-core",
            "@bureau/nodes-admin",
            "@bureau/nodes-core",
            "@bureau/offline-store",
        ],
    },
});
