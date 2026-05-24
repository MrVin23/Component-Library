// security risk: DEMO ONLY — revert localStorage theme persistence before production.
// Production: use server user settings only; restore prefers-color-scheme bootstrap in index.html if desired.
window.themeInterop = {
    storageKey: 'app-theme-preference',
    defaultTheme: 'light',

    getStoredTheme: function () {
        try {
            var stored = localStorage.getItem(this.storageKey);
            if (stored === 'light' || stored === 'dark') {
                return stored;
            }
        } catch (_) {
            // localStorage unavailable
        }
        return this.defaultTheme;
    },

    setTheme: function (theme) {
        if (theme !== 'light' && theme !== 'dark') {
            return;
        }

        document.documentElement.setAttribute('data-theme', theme);

        // security risk: DEMO ONLY — remove localStorage write before production.
        try {
            localStorage.setItem(this.storageKey, theme);
        } catch (_) {
            // localStorage unavailable
        }
    },

    isDarkMode: function () {
        return document.documentElement.getAttribute('data-theme') === 'dark';
    },

    init: function () {
        this.setTheme(this.getStoredTheme());
    },

    copyText: function (text) {
        return navigator.clipboard.writeText(text);
    }
};
