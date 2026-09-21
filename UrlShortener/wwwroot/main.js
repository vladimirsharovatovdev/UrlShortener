/**
 * jwt-auth.js
 * Handles JWT storage, attachment to requests, and basic expiry checks
 * for a Razor MVC app that uses JWT-secured API endpoints.
 *
 * Include this file in _Layout.cshtml:
 *   <script src="~/js/jwt-auth.js"></script>
 */

const JwtAuth = (function () {
    const STORAGE_KEY = "jwt_token";

    // ---- Storage ----

    function saveToken(token) {
        sessionStorage.setItem(STORAGE_KEY, token);
    }

    function getToken() {
        return sessionStorage.getItem(STORAGE_KEY);
    }

    function clearToken() {
        sessionStorage.removeItem(STORAGE_KEY);
    }

    // ---- Token inspection ----

    function decodeToken(token) {
        try {
            const payload = token.split(".")[1];
            const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
            return JSON.parse(decoded);
        } catch (err) {
            console.error("Failed to decode JWT:", err);
            return null;
        }
    }

    function isTokenExpired(token) {
        const decoded = decodeToken(token);
        if (!decoded || !decoded.exp) return true;

        const nowInSeconds = Math.floor(Date.now() / 1000);
        return decoded.exp < nowInSeconds;
    }

    function isAuthenticated() {
        const token = getToken();
        return !!token && !isTokenExpired(token);
    }

    // ---- Login / Logout ----

    async function login(name, password, loginUrl = "/manager-login") {
        const response = await fetch(loginUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name, password })
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || "Login failed");
        }

        const data = await response.json();
        if (!data.token) {
            throw new Error("No token returned from server");
        }

        saveToken(data.token);
        return data;
    }

    function logout(redirectUrl = "/") {
        clearToken();
        window.location.href = redirectUrl;
    }

    // ---- Authenticated fetch wrapper ----

    async function authFetch(url, options = {}) {
        const token = getToken();

        if (!token || isTokenExpired(token)) {
            clearToken();
            window.location.href = "/Auth/Login";
            return Promise.reject(new Error("No valid token, redirecting to login"));
        }

        const headers = {
            ...(options.headers || {}),
            "Authorization": `Bearer ${token}`
        };

        const response = await fetch(url, { ...options, headers });

        if (response.status === 401) {
            clearToken();
            window.location.href = "/Auth/Login";
        }

        return response;
    }

    function getUserName() {
        const token = getToken();
        if (!token || isTokenExpired(token)) return null;

        const decoded = decodeToken(token);
        if (!decoded) return null;

        // ASP.NET Core's ClaimTypes.Name maps to this long URI in the JWT payload,
        // but plain "name" or "unique_name" are common too depending on how the
        // token was issued. Check the common variants.
        return (
            decoded["name"] ||
            decoded["unique_name"] ||
            decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
            null
        );
    }

    return {
        saveToken,
        getToken,
        clearToken,
        isAuthenticated,
        isTokenExpired,
        login,
        logout,
        authFetch,
        getUserName
    };
})();