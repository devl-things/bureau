import { defineConfig } from "vite";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const shared = (name: string) =>
    path.resolve(__dirname, `../../../shared/${name}/src/index.ts`);
const libs = (name: string) =>
    path.resolve(__dirname, `../../libs/${name}/src/index.ts`);

export default defineConfig({
    server: {
        port: 5001,
        strictPort: true,
    },
    resolve: {
        alias: {
            "@bureau/chores-core": shared("chores-core"),
            "@bureau/client-core": libs("client-core"),
        },
    },
    optimizeDeps: {
        exclude: ["@bureau/chores-core", "@bureau/client-core"],
    },
});
