/**
 * ui.js — shared UI helpers for UrlShortener.
 * Requires main.js (JwtAuth) to be loaded first.
 *
 *   UI.toast(message, type)          toast notification (info | success | error)
 *   UI.flash(message, type)          show a toast on the NEXT page load
 *   UI.api(url, options)             fetch with auth header + friendly errors
 *   UI.fail(err, {status: message})  show an error toast for an api() failure
 *   UI.copy(text, button)            copy to clipboard with feedback
 *   UI.confirm(options)              modal confirm dialog -> Promise<boolean>
 *   UI.busy(button, asyncFn)         disable + spinner while asyncFn runs
 *   UI.requireAuth()                 redirect guests to the login page
 *   UI.shortLink(code)               absolute short URL for a code
 */
const UI = (function () {
    // ---- Icons (Lucide-style, stroke based) -------------------------------

    const ICONS = {
        link: '<path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/>',
        copy: '<rect width="14" height="14" x="8" y="8" rx="2" ry="2"/><path d="M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2"/>',
        check: '<path d="M20 6 9 17l-5-5"/>',
        external: '<path d="M15 3h6v6"/><path d="M10 14 21 3"/><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/>',
        trash: '<path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/>',
        plus: '<path d="M5 12h14"/><path d="M12 5v14"/>',
        search: '<circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>',
        eye: '<path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/>',
        eyeOff: '<path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/><path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><path d="m2 2 20 20"/>',
        logout: '<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><path d="m16 17 5-5-5-5"/><path d="M21 12H9"/>',
        x: '<path d="M18 6 6 18"/><path d="m6 6 12 12"/>',
        arrowRight: '<path d="M5 12h14"/><path d="m12 5 7 7-7 7"/>',
        arrowLeft: '<path d="m12 19-7-7 7-7"/><path d="M19 12H5"/>',
        chevronRight: '<path d="m9 18 6-6-6-6"/>',
        alert: '<circle cx="12" cy="12" r="10"/><path d="M12 8v4"/><path d="M12 16h.01"/>',
        checkCircle: '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><path d="m9 11 3 3L22 4"/>',
        info: '<circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/>',
        click: '<path d="M4.037 4.688a.495.495 0 0 1 .651-.651l16 6.5a.5.5 0 0 1-.063.947l-6.124 1.58a2 2 0 0 0-1.438 1.435l-1.579 6.126a.5.5 0 0 1-.947.063z"/>',
        zap: '<path d="M4 14a1 1 0 0 1-.78-1.63l9.9-10.2a.5.5 0 0 1 .86.46l-1.92 6.02A1 1 0 0 0 13 10h7a1 1 0 0 1 .78 1.63l-9.9 10.2a.5.5 0 0 1-.86-.46l1.92-6.02A1 1 0 0 0 11 14z"/>',
        hash: '<path d="M4 9h16"/><path d="M4 15h16"/><path d="M10 3 8 21"/><path d="m16 3-2 18"/>',
        globe: '<circle cx="12" cy="12" r="10"/><path d="M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20"/><path d="M2 12h20"/>'
    };

    function icon(name) {
        return '<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" ' +
            'stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + (ICONS[name] || "") + "</svg>";
    }

    /** Replace every <span data-icon="name"> inside root with an inline SVG. */
    function hydrateIcons(root) {
        (root || document).querySelectorAll("[data-icon]").forEach(function (el) {
            el.classList.add("ico");
            el.innerHTML = icon(el.getAttribute("data-icon"));
            el.removeAttribute("data-icon");
        });
    }

    // ---- Toasts -----------------------------------------------------------

    function toastContainer() {
        let el = document.getElementById("toasts");
        if (!el) {
            el = document.createElement("div");
            el.id = "toasts";
            el.className = "toasts";
            el.setAttribute("aria-live", "polite");
            document.body.appendChild(el);
        }
        return el;
    }

    function toast(message, type, opts) {
        type = type || "info";
        const duration = (opts && opts.duration) || (type === "error" ? 6500 : 4200);
        const iconName = type === "success" ? "checkCircle" : type === "error" ? "alert" : "info";

        const el = document.createElement("div");
        el.className = "toast toast-" + type;
        el.setAttribute("role", type === "error" ? "alert" : "status");
        el.innerHTML =
            '<span class="toast-icon">' + icon(iconName) + "</span>" +
            '<div class="toast-msg"></div>' +
            '<button type="button" class="icon-btn toast-close" aria-label="Dismiss">' + icon("x") + "</button>";
        el.querySelector(".toast-msg").textContent = message;

        function dismiss() {
            if (el.classList.contains("leaving")) return;
            el.classList.add("leaving");
            setTimeout(function () { el.remove(); }, 200);
        }
        el.querySelector(".toast-close").addEventListener("click", dismiss);
        toastContainer().appendChild(el);
        setTimeout(dismiss, duration);
        return el;
    }

    // Flash messages survive one page navigation.
    const FLASH_KEY = "ui_flash";

    function flash(message, type) {
        try { sessionStorage.setItem(FLASH_KEY, JSON.stringify({ message: message, type: type || "info" })); } catch (e) { /* ignore */ }
    }

    function showFlash() {
        try {
            const raw = sessionStorage.getItem(FLASH_KEY);
            if (!raw) return;
            sessionStorage.removeItem(FLASH_KEY);
            const f = JSON.parse(raw);
            if (f && f.message) toast(f.message, f.type);
        } catch (e) { /* ignore */ }
    }

    // ---- API --------------------------------------------------------------

    function defaultMessage(status) {
        if (status === 0) return "Can't reach the server. Check your connection and try again.";
        if (status === 400) return "The server rejected the request. Please check your input.";
        if (status === 401) return "Your session has expired. Please sign in again.";
        if (status === 403) return "You don't have permission to do that.";
        if (status === 404) return "Not found.";
        if (status >= 500) return "Server error. Please try again in a moment.";
        return "Something went wrong. Please try again.";
    }

    class ApiError extends Error {
        constructor(status, serverMessage) {
            // Only trust server-provided text for client errors (4xx); 5xx details may leak internals.
            const usable = status >= 400 && status < 500 ? serverMessage : "";
            super(usable || defaultMessage(status));
            this.name = "ApiError";
            this.status = status;
            this.serverMessage = usable || "";
            this.handled = false;
        }
    }

    async function readError(res) {
        let text = "";
        try { text = await res.text(); } catch (e) { return ""; }
        if (!text) return "";
        try {
            const j = JSON.parse(text);
            if (typeof j === "string") return j;
            if (j && j.errors) {
                const first = Object.values(j.errors).flat()[0];
                if (first) return String(first);
            }
            return (j && j.detail) || "";
        } catch (e) {
            return text.length < 200 ? text : "";
        }
    }

    /**
     * fetch wrapper. Adds JSON + Bearer headers, returns the Response on success,
     * throws ApiError otherwise. A 401 on an authenticated call clears the token
     * and sends the user to the login page.
     */
    async function api(url, options) {
        options = options || {};
        const method = options.method || "GET";
        const headers = {};
        let body;
        if (options.body !== undefined) {
            headers["Content-Type"] = "application/json";
            body = JSON.stringify(options.body);
        }
        if (options.auth !== false) {
            const token = JwtAuth.getToken();
            if (token) headers["Authorization"] = "Bearer " + token;
        }

        let res;
        try {
            res = await fetch(url, { method: method, headers: headers, body: body });
        } catch (e) {
            throw new ApiError(0);
        }

        if (res.status === 401 && options.auth !== false) {
            JwtAuth.clearToken();
            flash("Your session has expired. Please sign in again.", "error");
            window.location.href = "/login.html";
            const err = new ApiError(401);
            err.handled = true;
            throw err;
        }
        if (!res.ok) throw new ApiError(res.status, await readError(res));
        return res;
    }

    /** Show an error toast. `map` overrides the message per HTTP status. */
    function fail(err, map) {
        if (err && err.handled) return;
        const status = err && err.status;
        // A specific message from the server (e.g. a validation error) beats the generic per-status text.
        const msg = (err && err.serverMessage) ||
            (map && status !== undefined && map[status]) ||
            (err && err.message) || defaultMessage(-1);
        toast(msg, "error");
    }

    // ---- Helpers ----------------------------------------------------------

    function shortLink(code) {
        return window.location.origin + "/r/" + encodeURIComponent(code);
    }

    function shortLinkPrefix() {
        return window.location.host + "/r/";
    }

    async function copy(text, button) {
        let ok = false;
        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(text);
                ok = true;
            }
        } catch (e) { /* fall through */ }

        if (!ok) {
            const ta = document.createElement("textarea");
            ta.value = text;
            ta.setAttribute("readonly", "");
            ta.style.cssText = "position:fixed;top:-1000px;opacity:0";
            document.body.appendChild(ta);
            ta.select();
            try { ok = document.execCommand("copy"); } catch (e) { ok = false; }
            ta.remove();
        }

        if (!ok) {
            toast("Couldn't copy automatically. Please copy the link manually.", "error");
            return false;
        }

        toast("Link copied to clipboard", "success", { duration: 2200 });
        if (button) {
            const original = button.innerHTML;
            button.classList.add("copied");
            if (button.classList.contains("icon-btn")) button.innerHTML = icon("check");
            setTimeout(function () {
                button.classList.remove("copied");
                if (button.classList.contains("icon-btn")) button.innerHTML = original;
            }, 1400);
        }
        return true;
    }

    async function busy(button, fn) {
        button.disabled = true;
        button.classList.add("is-loading");
        try {
            return await fn();
        } finally {
            button.disabled = false;
            button.classList.remove("is-loading");
        }
    }

    function requireAuth() {
        if (JwtAuth.isAuthenticated()) return true;
        JwtAuth.clearToken();
        flash("Please sign in to continue.", "info");
        window.location.replace("/login.html");
        return false;
    }

    function confirmDialog(opts) {
        return new Promise(function (resolve) {
            const dlg = document.createElement("dialog");
            dlg.className = "modal";
            dlg.innerHTML =
                "<h3></h3><p></p>" +
                '<div class="form-actions">' +
                '<button type="button" class="btn btn-ghost" data-act="cancel">Cancel</button>' +
                '<button type="button" class="btn" data-act="ok"></button>' +
                "</div>";
            dlg.querySelector("h3").textContent = opts.title || "Are you sure?";
            dlg.querySelector("p").textContent = opts.message || "";
            const ok = dlg.querySelector('[data-act="ok"]');
            ok.textContent = opts.confirmText || "Confirm";
            ok.className = "btn " + (opts.danger ? "btn-danger solid" : "btn-primary");

            let result = false;
            dlg.addEventListener("click", function (e) {
                const act = e.target.closest && e.target.closest("[data-act]");
                if (act) { result = act.getAttribute("data-act") === "ok"; dlg.close(); }
                else if (e.target === dlg) dlg.close(); // backdrop click
            });
            dlg.addEventListener("close", function () { dlg.remove(); resolve(result); });
            document.body.appendChild(dlg);
            dlg.showModal();
            (opts.danger ? dlg.querySelector('[data-act="cancel"]') : ok).focus();
        });
    }

    /** Wire up every [data-toggle-password] button to reveal/hide its input. */
    function initPasswordToggles() {
        document.querySelectorAll("[data-toggle-password]").forEach(function (btn) {
            const input = document.getElementById(btn.getAttribute("data-toggle-password"));
            if (!input) return;
            btn.innerHTML = icon("eye");
            btn.addEventListener("click", function () {
                const show = input.type === "password";
                input.type = show ? "text" : "password";
                btn.innerHTML = icon(show ? "eyeOff" : "eye");
                btn.setAttribute("aria-label", show ? "Hide password" : "Show password");
            });
        });
    }

    // ---- Navigation -------------------------------------------------------

    function renderNav() {
        const el = document.getElementById("nav");
        if (!el) return;
        const page = document.body.getAttribute("data-page") || "";
        const authed = JwtAuth.isAuthenticated();
        const name = authed ? (JwtAuth.getUserName() || "Account") : "";

        el.classList.add("nav");
        el.innerHTML =
            '<div class="container nav-inner">' +
            '<a class="brand" href="/" aria-label="UrlShortener home">' +
            '<span class="brand-mark">' + icon("link") + "</span>" +
            '<span class="brand-name">Url<span>Shortener</span></span></a>' +
            '<nav class="nav-links">' +
            (authed
                ? '<a class="nav-link' + (page === "list" || page === "edit" ? " active" : "") + '" href="/short-urls.html">My links</a>'
                : "") +
            "</nav>" +
            '<div class="nav-actions">' +
            (authed
                ? '<span class="user-chip"><span class="avatar"></span><span class="name"></span></span>' +
                  '<button type="button" class="btn btn-ghost btn-sm" id="logout-button">' + icon("logout") + "<span>Logout</span></button>"
                : '<a class="btn btn-ghost btn-sm" href="/login.html">Login</a>' +
                  '<a class="btn btn-primary btn-sm" href="/register.html">Register</a>') +
            "</div></div>";

        if (authed) {
            el.querySelector(".avatar").textContent = name.trim().charAt(0) || "?";
            el.querySelector(".user-chip .name").textContent = name;
            el.querySelector(".user-chip").setAttribute("title", name);
            el.querySelector("#logout-button").addEventListener("click", function () { JwtAuth.logout(); });
        }
    }

    function init() {
        renderNav();
        hydrateIcons();
        initPasswordToggles();
        showFlash();
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();

    return {
        icon: icon,
        hydrateIcons: hydrateIcons,
        toast: toast,
        flash: flash,
        api: api,
        fail: fail,
        copy: copy,
        busy: busy,
        confirm: confirmDialog,
        requireAuth: requireAuth,
        shortLink: shortLink,
        shortLinkPrefix: shortLinkPrefix
    };
})();
