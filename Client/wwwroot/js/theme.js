window.themeInterop = {
    setTheme: function (theme) {
        if (theme === 'light' || theme === 'dark') {
            document.documentElement.setAttribute('data-theme', theme);
        }
    },
    isDarkMode: function () {
        return document.documentElement.getAttribute('data-theme') === 'dark';
    },
    copyText: function (text) {
        return navigator.clipboard.writeText(text);
    }
};
