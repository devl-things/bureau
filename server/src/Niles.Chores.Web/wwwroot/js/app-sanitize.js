// app-sanitize.js
(function (global) {
    "use strict";

    if (!global.DOMPurify) {
        console.error("DOMPurify is not loaded. HtmlSanitizer will not work.");
        return;
    }

    // Default config – good enough for most text-with-basic-formatting cases
    const DEFAULT_OPTIONS = Object.freeze({
        ALLOWED_TAGS: ["b", "strong", "i", "em", "u", "a", "br"],
        ALLOWED_ATTR: ["href", "title", "target", "rel"]
    });

    /**
     * Universal HTML sanitizer.
     *
     * @param {string} html         Raw HTML (user input, notes, etc.)
     * @param {Object|null} [options]    Optional DOMPurify config to override defaults
     * @returns {string}            Safe HTML string
     */
    function sanitize(html, options) {
        if (!html) {
            return "";
        }

        // Merge defaults with per-call options if provided
        const effectiveOptions = options
            ? ({ ...DEFAULT_OPTIONS, ...options})
            : DEFAULT_OPTIONS;

        return global.DOMPurify.sanitize(html, effectiveOptions);
    }

    // Expose a single, generic API
    global.HtmlSanitizer = {
        sanitize: sanitize
    };

}(globalThis));
