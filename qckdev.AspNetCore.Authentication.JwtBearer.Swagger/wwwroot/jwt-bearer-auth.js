(function () {
    "use strict";

    var config = parseClientConfigFromScript();
    console.log("[JwtBearerSwagger] 1.6 loader script executed");

    var PATCHED_FLAG = "__qckdevJwtSwaggerFetchPatched";

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

    function debugLog(rule) {
        if (!rule || !rule.log) {
            return;
        }

        var parts = Array.prototype.slice.call(arguments, 1);
        var detail = parts.length ? parts.join(" ") : "";
        console.debug("[JwtBearerSwagger]", rule.schemeName, "log=", rule.log, detail);
    }

    function normalizeMethod(method) {
        return ((method || "GET") + "").trim().toUpperCase();
    }

    function buildRuleMap(config) {
        var rules = (config && config.rules)
            ? config.rules
            : (config && config.Rules ? config.Rules : []);
        var map = {};
        for (var i = 0; i < rules.length; i++) {
            var rule = rules[i];
            if (!rule) {
                continue;
            }
            var schemeName = rule.schemeName || rule.SchemeName;
            var endpointPath = rule.endpointPath || rule.EndpointPath;
            if (!schemeName || !endpointPath) {
                continue;
            }

            var key = normalizeMethod(rule.httpMethod || rule.HttpMethod || "POST") + " " + normalizePath(endpointPath);
            map[key] = {
                schemeName: schemeName,
                endpointPath: endpointPath,
                httpMethod: normalizeMethod(rule.httpMethod || rule.HttpMethod || "POST"),
                tokenJsonPath: rule.tokenJsonPath || rule.TokenJsonPath || "accessToken",
                stripBearerPrefix: rule.stripBearerPrefix !== undefined
                    ? rule.stripBearerPrefix
                    : rule.StripBearerPrefix !== undefined
                        ? rule.StripBearerPrefix
                        : true,
                log: !!(rule.log || rule.Log)
            };
        }

        return map;
    }

    function getRequestUrl(input) {
        if (!input) {
            return "";
        }

        if (typeof input === "string") {
            return input;
        }

        if (typeof input.url === "string") {
            return input.url;
        }

        return "";
    }

    function getRequestMethod(input, init) {
        if (init && init.method) {
            return normalizeMethod(init.method);
        }

        if (input && input.method) {
            return normalizeMethod(input.method);
        }

        return "GET";
    }

    function getRequestPath(url) {
        if (!url) {
            return "/";
        }

        try {
            var full = new URL(url, window.location.origin);
            return normalizePath(full.pathname);
        } catch (_err) {
            return normalizePath(url.split("?")[0]);
        }
    }

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

    function normalizeToken(rawToken, stripBearerPrefix) {
        var value = (rawToken || "").toString().trim();
        if (!value) {
            return "";
        }

        if (stripBearerPrefix !== false && value.toLowerCase().indexOf("bearer ") === 0) {
            return value.substring(7).trim();
        }

        return value;
    }

    function applyAuthorization(schemeName, token) {
        if (!window.ui) {
            return;
        }

        if (typeof window.ui.preauthorizeApiKey === "function") {
            try {
                window.ui.preauthorizeApiKey(schemeName, token);
                return;
            } catch (_err) {
            }
        }

        if (!window.ui.getSystem) {
            return;
        }

        var system = window.ui.getSystem();
        var authActions = system && system.authActions ? system.authActions : null;
        if (!authActions || typeof authActions.authorize !== "function") {
            return;
        }

        var payload = {};
        payload[schemeName] = {
            name: schemeName,
            value: token
        };

        authActions.authorize(payload);
    }

    function processResponse(rule, response) {
        if (!response || !response.ok) {
            return;
        }

        var contentType = (response.headers && response.headers.get("content-type")) || "";
        debugLog(rule, "response content-type", contentType);
        if (contentType.toLowerCase().indexOf("application/json") < 0) {
            return;
        }

        response.clone().json()
            .then(function (payload) {
                debugLog(rule, "response payload parsed");
                var rawToken = getValueByPath(payload, rule.tokenJsonPath || "accessToken");
                var token = normalizeToken(rawToken, rule.stripBearerPrefix);
                if (!token) {
                    debugLog(rule, "token not found at path", rule.tokenJsonPath || "accessToken");
                    return;
                }

                debugLog(rule, "token extracted", token);
                applyAuthorization(rule.schemeName, token);
            })
            .catch(function () {
                debugLog(rule, "failed to parse JSON response");
            });
    }

    function patchFetch(ruleMap) {
        if (!window.fetch || window[PATCHED_FLAG]) {
            return;
        }

        var originalFetch = window.fetch.bind(window);
        window.fetch = function (input, init) {
            var method = getRequestMethod(input, init);
            var path = getRequestPath(getRequestUrl(input));
            var ruleKey = method + " " + path;
            var rule = ruleMap[ruleKey];

            return originalFetch(input, init).then(function (response) {
                if (rule) {
                    debugLog(rule, "matched request", method, path);
                    processResponse(rule, response);
                }

                return response;
            });
        };

        window[PATCHED_FLAG] = true;
        console.log("[JwtBearerSwagger] fetch patched");
    }

    function waitForSwaggerUiAndPatch(ruleMap) {
        var attempts = 0;
        var timer = setInterval(function () {
            attempts++;
        if (window.fetch) {
            clearInterval(timer);
            patchFetch(ruleMap);
            return;
        }

            if (attempts > 200) {
                clearInterval(timer);
                console.warn("[JwtBearerSwagger] stopped waiting for Swagger UI after 200 attempts");
            }
        }, 100);
    }

    var ruleMap = buildRuleMap(config);
    if (Object.keys(ruleMap).length > 0) {
        waitForSwaggerUiAndPatch(ruleMap);
    }
})();
