export interface AuthUser {
    name: string;
}

export interface IAuthProvider {
    readonly user: AuthUser | null;
    readonly featureScopes: string[];
    getAccessTokenAsync(): Promise<string | null>;
}
