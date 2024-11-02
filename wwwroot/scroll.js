window.scrollHelper = {
    saveScrollPosition: function () {
        sessionStorage.setItem("scrollPos", window.scrollY);
    },
    restoreScrollPosition: function () {
        const scrollPos = sessionStorage.getItem("scrollPos");
        if (scrollPos) {
            window.scrollTo(0, parseInt(scrollPos, 10));
            sessionStorage.removeItem("scrollPos");
        }
    }
};