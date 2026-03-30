(function () {
    "use strict";

    var PATCH_FLAGS = {
        fetch: "__qckdevJwtSwaggerFetchPatched",
        xhr: "__qckdevJwtSwaggerXhrPatched"
    };

    console.log("[JwtBearerSwagger] 1.10 loader script executed");

    var config = parseClientConfigFromScript();
    var rules = buildRules(config);

    if (rules.length === 0) {
        return;
    }

    waitForSwaggerUi(function () {
        resolveBasePaths()
            .then(function (basePaths) {
                var ruleLookup = buildRuleLookup(rules, basePaths);
                patchFetch(ruleLookup);
                patchXhr(ruleLookup);
            })
            .catch(function () {
                var fallbackLookup = buildRuleLookup(rules, ["/"]);
                patchFetch(fallbackLookup);
                patchXhr(fallbackLookup);
            });
    });

    // ---------------------------------
    // Config parsing
    // ---------------------------------

    /**
     * Returns the URL of the currently executing helper script.
     * Falls back to scanning script tags when document.currentScript is unavailable.
     * @returns {string}
     */
    function getCurrentScriptSource() {
        if (document.currentScript && document.currentScript.src) {
            return document.currentScript.src;
        }

        var scripts = document.querySelectorAll("script[src]");
        for (var i = scripts.length - 1; i >= 0; i--) {
            var src = scripts[i].getAttribute("src") || "";
            if (src.indexOf("jwt-bearer-auth.js") >= 0) {
                return src;
            }
        }

        return "";
    }

    /**
     * Reads and parses the serialized helper configuration from the script query string.
     * @returns {{ rules: Array }}
     */
    function parseClientConfigFromScript() {
        var src = getCurrentScriptSource();
        if (!src) {
            return { rules: [] };
        }

        var queryIndex = src.indexOf("?");
        if (queryIndex < 0) {
            return { rules: [] };
        }

        var query = src.substring(queryIndex + 1);
        var parts = query.split("&");
        for (var i = 0; i < parts.length; i++) {
            var pair = parts[i].split("=");
            if (pair.length < 2 || pair[0] !== "cfg") {
                continue;
            }

            try {
                return JSON.parse(decodeURIComponent(pair.slice(1).join("=")));
            } catch (_err) {
                return { rules: [] };
            }
        }

        return { rules: [] };
    }

    /**
     * Normalizes and validates capture rules from the incoming client config.
     * @param {{ rules?: Array, Rules?: Array }} parsedConfig
     * @returns {Array<{schemeName:string, endpointPath:string, httpMethod:string, tokenJsonPath:string, stripBearerPrefix:boolean, log:boolean}>}
     */
    function buildRules(parsedConfig) {
        var sourceRules = (parsedConfig && parsedConfig.rules)
            ? parsedConfig.rules
            : (parsedConfig && parsedConfig.Rules ? parsedConfig.Rules : []);

        var result = [];
        for (var i = 0; i < sourceRules.length; i++) {
            var source = sourceRules[i];
            if (!source) {
                continue;
            }

            var schemeName = source.schemeName || source.SchemeName;
            var endpointPath = source.endpointPath || source.EndpointPath;
            if (!schemeName || !endpointPath) {
                continue;
            }

            result.push({
                schemeName: schemeName,
                endpointPath: normalizePath(endpointPath),
                httpMethod: normalizeMethod(source.httpMethod || source.HttpMethod || "POST"),
                tokenJsonPath: source.tokenJsonPath || source.TokenJsonPath || "accessToken",
                stripBearerPrefix: source.stripBearerPrefix !== undefined
                    ? source.stripBearerPrefix
                    : source.StripBearerPrefix !== undefined
                        ? source.StripBearerPrefix
                        : true,
                log: !!(source.log || source.Log)
            });
        }

        return result;
    }

    // ---------------------------------
    // Paths / base path resolution
    // ---------------------------------

    /**
     * Normalizes paths to lower-case absolute format without trailing slash.
     * @param {string} path
     * @returns {string}
     */
    function normalizePath(path) {
        var value = (path || "").toString().trim();
        if (!value) {
            return "/";
        }

        if (!value.startsWith("/")) {
            value = "/" + value;
        }

        if (value.length > 1 && value.endsWith("/")) {
            value = value.substring(0, value.length - 1);
        }

        return value.toLowerCase();
    }

    /**
     * Normalizes HTTP method names to upper-case.
     * @param {string} method
     * @returns {string}
     */
    function normalizeMethod(method) {
        return ((method || "GET") + "").trim().toUpperCase();
    }

    /**
     * Combines a base path with an endpoint path into a normalized absolute path.
     * @param {string} basePath
     * @param {string} endpointPath
     * @returns {string}
     */
    function combinePaths(basePath, endpointPath) {
        var left = normalizePath(basePath);
        var right = normalizePath(endpointPath);

        if (left === "/") {
            return right;
        }

        if (right === "/") {
            return left;
        }

        return normalizePath(left + "/" + right.substring(1));
    }

    /**
     * Adds a value only when it does not already exist in the array.
     * @param {string[]} values
     * @param {string} value
     */
    function addDistinct(values, value) {
        for (var i = 0; i < values.length; i++) {
            if (values[i] === value) {
                return;
            }
        }

        values.push(value);
    }

    /**
     * Resolves the current Swagger/OpenAPI document URL from Swagger UI configs.
     * @returns {string}
     */
    function getSwaggerSpecUrl() {
        if (!window.ui || typeof window.ui.getConfigs !== "function") {
            return "";
        }

        var configs = window.ui.getConfigs() || {};
        if (typeof configs.url === "string" && configs.url) {
            return configs.url;
        }

        var urls = Array.isArray(configs.urls) ? configs.urls : [];
        if (urls.length === 0) {
            return "";
        }

        for (var i = 0; i < urls.length; i++) {
            var entry = urls[i];
            if (!entry || typeof entry.url !== "string" || !entry.url) {
                continue;
            }

            if (configs.urlsPrimaryName && entry.name === configs.urlsPrimaryName) {
                return entry.url;
            }
        }

        return urls[0] && urls[0].url ? urls[0].url : "";
    }

    /**
     * Extracts the base path prefix before '/swagger' from a URL pathname.
     * @param {string} pathname
     * @returns {string}
     */
    function tryExtractBasePathBeforeSwagger(pathname) {
        var normalized = normalizePath(pathname);
        if (!normalized || normalized === "/") {
            return "";
        }

        var markerIndex = normalized.indexOf("/swagger");
        if (markerIndex < 0) {
            return "";
        }

        var prefix = normalized.substring(0, markerIndex);
        return prefix ? normalizePath(prefix) : "/";
    }

    /**
     * Infers candidate base paths from current UI URL and swagger.json URL.
     * @param {string} specUrl
     * @returns {string[]}
     */
    function inferBasePaths(specUrl) {
        var inferred = ["/"];

        var fromUi = tryExtractBasePathBeforeSwagger(window.location.pathname || "/");
        if (fromUi) {
            addDistinct(inferred, fromUi);
        }

        if (specUrl) {
            try {
                var specPath = new URL(specUrl, window.location.origin).pathname || "/";
                var fromSpec = tryExtractBasePathBeforeSwagger(specPath);
                if (fromSpec) {
                    addDistinct(inferred, fromSpec);
                }
            } catch (_err) {
            }
        }

        return inferred;
    }

    /**
     * Attempts to read the OpenAPI document directly from Swagger UI runtime state.
     * @returns {object|null}
     */
    function getSpecFromSwaggerUiState() {
        if (!window.ui || typeof window.ui.getSystem !== "function") {
            return null;
        }

        var system = window.ui.getSystem();
        if (!system || !system.specSelectors || typeof system.specSelectors.specJson !== "function") {
            return null;
        }

        var spec = system.specSelectors.specJson();
        if (!spec) {
            return null;
        }

        if (typeof spec.toJS === "function") {
            try {
                return spec.toJS();
            } catch (_err) {
                return null;
            }
        }

        return spec;
    }

    /**
     * Extracts base paths from OpenAPI servers entries.
     * Ignores templated server URLs to avoid ambiguous matching.
     * @param {any} spec
     * @param {string} sourceUrl
     * @returns {string[]}
     */
    function parseServerBasePaths(spec, sourceUrl) {
        var basePaths = ["/"];
        if (!spec || !Array.isArray(spec.servers)) {
            return basePaths;
        }

        for (var i = 0; i < spec.servers.length; i++) {
            var server = spec.servers[i];
            var serverUrl = server && typeof server.url === "string" ? server.url : "";
            if (!serverUrl || serverUrl.indexOf("{") >= 0) {
                continue;
            }

            try {
                var absolute = new URL(serverUrl, sourceUrl || window.location.origin);
                addDistinct(basePaths, normalizePath(absolute.pathname || "/"));
            } catch (_err) {
            }
        }

        return basePaths;
    }

    /**
     * Merges two base path collections and de-duplicates entries.
     * @param {string[]} primary
     * @param {string[]} extra
     * @returns {string[]}
     */
    function mergeBasePaths(primary, extra) {
        var merged = primary.slice();
        for (var i = 0; i < extra.length; i++) {
            addDistinct(merged, normalizePath(extra[i]));
        }

        return merged;
    }

    /**
     * Resolves canonical base paths using Swagger state, spec servers, and URL inference.
     * @returns {Promise<string[]>}
     */
    function resolveBasePaths() {
        var specUrl = getSwaggerSpecUrl();
        var inferred = inferBasePaths(specUrl);

        var inMemorySpec = getSpecFromSwaggerUiState();
        if (inMemorySpec) {
            return Promise.resolve(mergeBasePaths(parseServerBasePaths(inMemorySpec, specUrl), inferred));
        }

        if (!specUrl || !window.fetch) {
            return Promise.resolve(inferred);
        }

        var absoluteSpecUrl;
        try {
            absoluteSpecUrl = new URL(specUrl, window.location.origin).toString();
        } catch (_err) {
            return Promise.resolve(inferred);
        }

        return window.fetch(absoluteSpecUrl)
            .then(function (response) {
                if (!response || !response.ok) {
                    return null;
                }

                return response.json();
            })
            .then(function (spec) {
                return mergeBasePaths(parseServerBasePaths(spec, absoluteSpecUrl), inferred);
            })
            .catch(function () {
                return inferred;
            });
    }

    // ---------------------------------
    // Rule lookup
    // ---------------------------------

    /**
     * Builds a lookup keyed by METHOD + canonical PATH for fast request matching.
     * @param {Array} rulesList
     * @param {string[]} basePaths
     * @returns {Record<string, any>}
     */
    function buildRuleLookup(rulesList, basePaths) {
        var lookup = {};
        var safeBasePaths = Array.isArray(basePaths) && basePaths.length > 0 ? basePaths : ["/"];

        for (var i = 0; i < rulesList.length; i++) {
            var rule = rulesList[i];
            var endpointPath = normalizePath(rule.endpointPath);

            lookup[rule.httpMethod + " " + endpointPath] = rule;

            for (var j = 0; j < safeBasePaths.length; j++) {
                var canonicalPath = combinePaths(safeBasePaths[j], endpointPath);
                lookup[rule.httpMethod + " " + canonicalPath] = rule;
            }
        }

        return lookup;
    }

    /**
     * Finds the matching capture rule for a request method/path pair.
     * @param {Record<string, any>} lookup
     * @param {string} method
     * @param {string} path
     * @returns {any|null}
     */
    function findRule(lookup, method, path) {
        return lookup[normalizeMethod(method) + " " + normalizePath(path)] || null;
    }

    /**
     * Converts request URL input into a normalized request pathname.
     * @param {string} url
     * @returns {string}
     */
    function getRequestPath(url) {
        if (!url) {
            return "/";
        }

        try {
            return normalizePath(new URL(url, window.location.origin).pathname);
        } catch (_err) {
            return normalizePath((url + "").split("?")[0]);
        }
    }

    // ---------------------------------
    // Payload/token handling
    // ---------------------------------

    /**
     * Emits scoped debug logs only for rules with log enabled.
     * @param {any} rule
     */
    function debugLog(rule) {
        if (!rule || !rule.log) {
            return;
        }

        var args = Array.prototype.slice.call(arguments, 1);
        var detail = args.length > 0 ? args.join(" ") : "";
        console.log("[JwtBearerSwagger]", rule.schemeName, detail);
    }

    /**
     * Checks whether a content-type value represents JSON payloads.
     * @param {string} contentType
     * @returns {boolean}
     */
    function isJsonContentType(contentType) {
        return (contentType || "").toLowerCase().indexOf("json") >= 0;
    }

    /**
     * Reads a value from JSON object using dot notation path.
     * @param {any} payload
     * @param {string} path
     * @returns {any}
     */
    function getValueByPath(payload, path) {
        if (!path) {
            return payload;
        }

        var current = payload;
        var parts = path.split(".");
        for (var i = 0; i < parts.length; i++) {
            var key = parts[i];
            if (!key) {
                continue;
            }

            if (current == null || typeof current !== "object" || !(key in current)) {
                return null;
            }

            current = current[key];
        }

        return current;
    }

    /**
     * Normalizes token string and optionally strips leading 'Bearer ' prefix.
     * @param {any} rawToken
     * @param {boolean} stripBearerPrefix
     * @returns {string}
     */
    function normalizeToken(rawToken, stripBearerPrefix) {
        var token = (rawToken || "").toString().trim();
        if (!token) {
            return "";
        }

        if (stripBearerPrefix !== false && token.toLowerCase().indexOf("bearer ") === 0) {
            return token.substring(7).trim();
        }

        return token;
    }

    /**
     * Shared response pipeline for fetch/XHR adapters: parse JSON, extract token and authorize.
     * @param {any} rule
     * @param {{isSuccess:boolean, contentType:string, readJson: () => Promise<any>}} responseAdapter
     */
    function handleMatchedResponse(rule, responseAdapter) {
        if (!responseAdapter || !responseAdapter.isSuccess || !isJsonContentType(responseAdapter.contentType)) {
            return;
        }

        responseAdapter.readJson()
            .then(function (payload) {
                var rawToken = getValueByPath(payload, rule.tokenJsonPath || "accessToken");
                var token = normalizeToken(rawToken, rule.stripBearerPrefix);
                if (!token) {
                    debugLog(rule, "token not found at", rule.tokenJsonPath || "accessToken");
                    return;
                }

                debugLog(rule, "token extracted");
                applyAuthorization(rule, token);
            })
            .catch(function () {
                debugLog(rule, "response is not valid JSON");
            });
    }

    // ---------------------------------
    // Swagger authorize
    // ---------------------------------

    /**
     * Tries to resolve a security scheme definition from Swagger UI system selectors.
     * Supports different selector names between Swagger UI versions.
     * @param {any} system
     * @param {string} schemeName
     * @returns {any|null}
     */
    function tryGetSecurityScheme(system, schemeName) {
        if (!system || !system.specSelectors) {
            return null;
        }

        var selector = null;
        if (typeof system.specSelectors.securityDefinitions === "function") {
            selector = system.specSelectors.securityDefinitions();
        } else if (typeof system.specSelectors.securitySchemes === "function") {
            selector = system.specSelectors.securitySchemes();
        }

        if (!selector) {
            return null;
        }

        var schema = typeof selector.get === "function" ? selector.get(schemeName) : selector[schemeName];
        if (!schema) {
            return null;
        }

        if (typeof schema.toJS === "function") {
            try {
                return schema.toJS();
            } catch (_err) {
                return null;
            }
        }

        return schema;
    }

    /**
     * Attempts Swagger UI preauthorization using preauthorizeApiKey API.
     * @param {string} schemeName
     * @param {string} value
     * @param {any} rule
     * @returns {boolean}
     */
    function tryPreauthorizeApiKey(schemeName, value, rule) {
        if (!window.ui || typeof window.ui.preauthorizeApiKey !== "function") {
            return false;
        }

        try {
            window.ui.preauthorizeApiKey(schemeName, value);
            debugLog(rule, "preauthorizeApiKey applied");
            return true;
        } catch (_err) {
            return false;
        }
    }

    /**
     * Attempts Swagger UI authorization through authActions.authorize fallback API.
     * @param {any} system
     * @param {string} schemeName
     * @param {string} value
     * @param {boolean} includeSchema
     * @param {any} rule
     * @returns {boolean}
     */
    function tryAuthorizeAction(system, schemeName, value, includeSchema, rule) {
        var authActions = system && system.authActions ? system.authActions : null;
        if (!authActions || typeof authActions.authorize !== "function") {
            return false;
        }

        var payloadEntry = {
            name: schemeName,
            value: value
        };

        if (includeSchema) {
            var schema = tryGetSecurityScheme(system, schemeName);
            if (schema) {
                payloadEntry.schema = schema;
            }
        }

        var payload = {};
        payload[schemeName] = payloadEntry;

        try {
            authActions.authorize(payload);
            debugLog(rule, "authActions.authorize applied");
            return true;
        } catch (_err) {
            return false;
        }
    }

    /**
     * Applies token to Swagger authorization with robust fallback order.
     * @param {any} rule
     * @param {string} token
     */
    function applyAuthorization(rule, token) {
        var schemeName = rule.schemeName;

        if (tryPreauthorizeApiKey(schemeName, token, rule)) {
            return;
        }

        if (tryPreauthorizeApiKey(schemeName, "Bearer " + token, rule)) {
            return;
        }

        if (!window.ui || typeof window.ui.getSystem !== "function") {
            return;
        }

        var system = window.ui.getSystem();
        if (!system) {
            return;
        }

        if (tryAuthorizeAction(system, schemeName, token, true, rule)) {
            return;
        }

        tryAuthorizeAction(system, schemeName, "Bearer " + token, true, rule);
    }

    // ---------------------------------
    // Transport hooks
    // ---------------------------------

    /**
     * Patches window.fetch to inspect matching token responses.
     * @param {Record<string, any>} ruleLookup
     */
    function patchFetch(ruleLookup) {
        if (!window.fetch || window[PATCH_FLAGS.fetch]) {
            return;
        }

        var originalFetch = window.fetch.bind(window);

        window.fetch = function (input, init) {
            var method = normalizeMethod((init && init.method) || (input && input.method) || "GET");
            var url = typeof input === "string" ? input : (input && input.url ? input.url : "");
            var path = getRequestPath(url);
            var rule = findRule(ruleLookup, method, path);

            return originalFetch(input, init).then(function (response) {
                if (rule) {
                    debugLog(rule, "matched fetch", method, path);
                    handleMatchedResponse(rule, {
                        isSuccess: !!(response && response.ok),
                        contentType: response && response.headers ? (response.headers.get("content-type") || "") : "",
                        readJson: function () {
                            return response.clone().json();
                        }
                    });
                }

                return response;
            });
        };

        window[PATCH_FLAGS.fetch] = true;
    }

    /**
     * Patches XMLHttpRequest to inspect matching token responses.
     * @param {Record<string, any>} ruleLookup
     */
    function patchXhr(ruleLookup) {
        if (!window.XMLHttpRequest || window[PATCH_FLAGS.xhr]) {
            return;
        }

        var proto = window.XMLHttpRequest.prototype;
        if (!proto || typeof proto.open !== "function" || typeof proto.send !== "function") {
            return;
        }

        var originalOpen = proto.open;
        var originalSend = proto.send;

        proto.open = function (method, url) {
            this.__qckdevJwtMethod = normalizeMethod(method);
            this.__qckdevJwtPath = getRequestPath(url || "");
            return originalOpen.apply(this, arguments);
        };

        proto.send = function () {
            var xhr = this;
            if (!xhr.__qckdevJwtHooked) {
                xhr.__qckdevJwtHooked = true;
                xhr.addEventListener("loadend", function () {
                    var method = xhr.__qckdevJwtMethod || "GET";
                    var path = xhr.__qckdevJwtPath || "/";
                    var rule = findRule(ruleLookup, method, path);

                    if (!rule) {
                        return;
                    }

                    debugLog(rule, "matched xhr", method, path);
                    handleMatchedResponse(rule, {
                        isSuccess: xhr.status >= 200 && xhr.status < 300,
                        contentType: xhr.getResponseHeader ? (xhr.getResponseHeader("content-type") || "") : "",
                        readJson: function () {
                            if (!xhr.responseText) {
                                return Promise.reject(new Error("Empty response"));
                            }

                            return Promise.resolve(JSON.parse(xhr.responseText));
                        }
                    });
                });
            }

            return originalSend.apply(this, arguments);
        };

        window[PATCH_FLAGS.xhr] = true;
    }

    /**
     * Waits until Swagger UI runtime object is available, then executes callback.
     * @param {() => void} onReady
     */
    function waitForSwaggerUi(onReady) {
        var attempts = 0;
        var timer = setInterval(function () {
            attempts++;

            if (window.ui) {
                clearInterval(timer);
                onReady();
                return;
            }

            if (attempts > 200) {
                clearInterval(timer);
                console.warn("[JwtBearerSwagger] Swagger UI was not detected after 200 attempts");
            }
        }, 100);
    }
})();
