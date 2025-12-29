class g {
  static loadFromWindow() {
    const t = window.__BUREAU__;
    if (t == null || typeof t != "object")
      throw new Error("Missing window.__BUREAU__ runtime configuration.");
    const e = t, n = e.environment, i = e.version, r = e.apiBaseUrls, l = e.defaultApiKey;
    if (typeof n != "string" || n.trim().length === 0)
      throw new Error("Invalid runtime config: 'environment' is required.");
    if (typeof i != "string" || i.trim().length === 0)
      throw new Error("Invalid runtime config: 'version' is required.");
    if (r == null || typeof r != "object")
      throw new Error("Invalid runtime config: 'apiBaseUrls' is required.");
    const a = {}, s = r;
    for (const u of Object.keys(s)) {
      const d = s[u];
      typeof d == "string" && d.trim().length > 0 && (a[u] = d);
    }
    if (Object.keys(a).length === 0)
      throw new Error("Invalid runtime config: 'apiBaseUrls' must contain at least one entry.");
    const o = {
      environment: n.trim(),
      version: i.trim(),
      apiBaseUrls: a
    };
    return typeof l == "string" && l.trim().length > 0 && (o.defaultApiKey = l.trim()), o;
  }
}
class y {
  async getAccessTokenAsync() {
    return null;
  }
}
class p {
  static createBearer(t) {
    if (t === null) return null;
    const e = t.trim();
    return e.length === 0 ? null : `Bearer ${e}`;
  }
}
class f extends Error {
  constructor(t, e, n, i, r) {
    super(t), this.name = "ApiError", this.status = e, this.request = n, this.problem = i, this.bodyText = r;
  }
}
function h(c) {
  if (c === null) return !1;
  const t = c.toLowerCase();
  return t.includes("application/json") || t.includes("application/problem+json");
}
class w {
  constructor(t, e) {
    this._config = t, this._tokenProvider = e ?? new y();
  }
  async getAsync(t) {
    return await this.requestAsync({ ...t, method: "GET" });
  }
  async postAsync(t) {
    return await this.requestAsync({ ...t, method: "POST" });
  }
  async requestAsync(t) {
    const e = t.method ?? "GET", n = this.resolveBaseUrl(t.apiKey), i = this.combineUrl(n, t.path), r = {
      ...t.headers ?? {}
    }, l = await this._tokenProvider.getAccessTokenAsync(), a = p.createBearer(l);
    a !== null && (r.Authorization = a);
    let s;
    t.body !== void 0 && ("Content-Type" in r || (r["Content-Type"] = "application/json"), s = JSON.stringify(t.body));
    const o = await fetch(i, {
      method: e,
      headers: r,
      body: s,
      signal: t.signal,
      credentials: t.credentials ?? "same-origin"
    });
    if (o.status === 204)
      return;
    const u = o.headers.get("content-type"), d = { method: e, url: i };
    if (!o.ok)
      throw await this.createApiErrorAsync(o, d, u);
    return h(u) ? await o.json() : await o.text();
  }
  resolveBaseUrl(t) {
    const e = t ?? this._config.defaultApiKey;
    if (e === void 0 || e.trim().length === 0)
      throw new Error("ApiClient: apiKey is required (no defaultApiKey configured).");
    const n = this._config.apiBaseUrls[e];
    if (n === void 0 || n.trim().length === 0)
      throw new Error(`ApiClient: apiBaseUrls does not contain key '${e}'.`);
    return n.trim();
  }
  combineUrl(t, e) {
    const n = t.endsWith("/") ? t.slice(0, -1) : t, i = e.startsWith("/") ? e : `/${e}`;
    return `${n}${i}`;
  }
  async createApiErrorAsync(t, e, n) {
    const i = t.status;
    if (h(n))
      try {
        const a = await t.json(), s = this.tryAsProblemDetails(a), o = (s == null ? void 0 : s.title) ?? (s == null ? void 0 : s.detail) ?? `Request failed with status ${i}.`;
        return new f(o, i, e, s, void 0);
      } catch {
      }
    const r = await t.text().catch(() => ""), l = r.length > 0 ? r : `Request failed with status ${i}.`;
    return new f(l, i, e, void 0, r);
  }
  tryAsProblemDetails(t) {
    if (t == null || typeof t != "object") return;
    const e = t, n = typeof e.title == "string", i = typeof e.status == "number";
    if (!(!n && !i))
      return e;
  }
}
export {
  w as ApiClient,
  f as ApiError,
  p as AuthorizationHeaderFactory,
  g as ClientRuntimeConfigLoader,
  y as NullAccessTokenProvider
};
