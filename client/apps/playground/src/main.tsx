import React from "react";
import ReactDOM from "react-dom/client";
import { ClientRuntimeConfigLoader } from "@bureau/client-core";

function App(): JSX.Element {
    const [text, setText] = React.useState<string>("");

    React.useEffect(() => {
        console.log("main.tsx loaded");
        // For now, simulate the host injecting runtime config
        (window as unknown as { __BUREAU__?: unknown }).__BUREAU__ = {
            environment: "dev",
            version: "local",
            apiBaseUrls: { demo: "http://localhost:1234" },
            defaultApiKey: "demo"
        };

        const cfg = ClientRuntimeConfigLoader.loadFromWindow();
        setText(`Loaded runtime config: env=${cfg.environment}, version=${cfg.version}, keys=${Object.keys(cfg.apiBaseUrls).join(",")}`);
    }, []);

    return (
        <div style={{ fontFamily: "system-ui", padding: 20 }}>
            <h1>Playground</h1>
            <p>{text}</p>
        </div>
    );
}

ReactDOM.createRoot(document.getElementById("root")!).render(<App />);
