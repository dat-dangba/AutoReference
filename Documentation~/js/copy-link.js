(function () {
    function setTempLabel(btn, text) {
        const old = btn.textContent;
        btn.textContent = text;
        setTimeout(() => {
            btn.textContent = old;
        }, 1200);
    }

    async function handleClick(e) {
        const btn = e.currentTarget;
        const text = btn.getAttribute('data-copy') || '';
        if (!text) return;

        try {
            await navigator.clipboard.writeText(text);
            setTempLabel(btn, 'Copied');
        } catch {
            setTempLabel(btn, 'Copy failed');
        }
    }

    function init() {
        document.querySelectorAll('button.copy-link[data-copy]')
            .forEach(btn => btn.addEventListener('click', handleClick));
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
