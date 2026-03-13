import type { IAccessTokenProvider } from "./IAccessTokenProvider";

export class NullAccessTokenProvider implements IAccessTokenProvider {
    public async getAccessTokenAsync(): Promise<string | null> {
        return null;
    }
}
