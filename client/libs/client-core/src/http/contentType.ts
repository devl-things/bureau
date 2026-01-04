export function isJsonContentType(contentType: string | null): boolean {
    if (contentType === null) return false;

    const normalized: string = contentType.toLowerCase();
    return normalized.includes("application/json") || normalized.includes("application/problem+json");
}
