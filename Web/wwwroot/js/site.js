// BiblioPlus — sidebar toggle for mobile.
(function () {
    var body = document.body;

    function open() { body.classList.add('nav-open'); }
    function close() { body.classList.remove('nav-open'); }
    function toggle() { body.classList.toggle('nav-open'); }

    document.addEventListener('click', function (e) {
        if (e.target.closest('[data-nav-toggle]')) { toggle(); return; }
        if (e.target.closest('[data-nav-close]')) { close(); return; }
        // close when a nav link is followed on mobile
        if (e.target.closest('.side-link')) { close(); }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') close();
    });
})();
