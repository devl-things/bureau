export class AuthorizationHeaderFactory {
    public static createBearer(token: string | null): string | null {
        if (token === null) return null;

        const trimmed = token.trim();
        if (trimmed.length === 0) return null;

        return `Bearer ${trimmed}`;
    }
}
