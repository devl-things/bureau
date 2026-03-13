import { useContext } from "react";
import { AuthContext } from "./AuthContext";
import type { IAuthProvider } from "./IAuthProvider";

export function useAuth(): IAuthProvider {
    const auth = useContext(AuthContext);
    if (!auth) throw new Error("useAuth must be used within AuthProvider");
    return auth;
}
