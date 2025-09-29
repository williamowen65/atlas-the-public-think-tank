document.addEventListener("DOMContentLoaded", () => {

    const LS_KEY = "globalCompositeScopesEnabled";

    function applyGlobalCompositeScopeToCards(enabled) {
        const allVisibleCards = document.querySelectorAll(".card.issue-card:not(.select2-item), .card.solution-card:not(.select2-item)");
        allVisibleCards.forEach(card => {
            if (enabled) {
                card.classList.add("show-composite-scope");
            } else {
                card.classList.remove("show-composite-scope");
            }
        });
    }


    // Set listener for global composite scope toggle
    const globalCompositeScopesToggle = document.querySelector("#globalCompositeScopesToggle");
    if (!globalCompositeScopesToggle) {
        return console.warn("Global composite scope button not found.")
    }

    // Initialize from localStorage
    const stored = localStorage.getItem(LS_KEY);
    const onloadScopeSetting = stored === "true";
    globalCompositeScopesToggle.checked = onloadScopeSetting;
    
    applyGlobalCompositeScopeToCards(onloadScopeSetting);

    globalCompositeScopesToggle.addEventListener("change", (e) => {
        const enabled = e.target.checked; // boolean for checkbox
        // save it in the local storage
        localStorage.setItem(LS_KEY, enabled.toString());
        // update all the cards on the page
        applyGlobalCompositeScopeToCards(enabled);
    });

    // If a custom documentObserver system exists, register so dynamically added cards also get updated
    if (typeof documentObserver === 'object' && typeof documentObserver.registerEvent === 'function') {
        documentObserver.registerEvent(docObserverCompositeScope);
    }

    function docObserverCompositeScope(node) {
        if (!(node instanceof Element)) return;
        const stored = localStorage.getItem(LS_KEY);
        const currentScopeSetting = stored === "true";
        if (!currentScopeSetting) return;

        // If node itself is a card, apply the class
        if (node.classList && (node.classList.contains('issue-card:not(.select2-item)') || node.classList.contains('solution-card:not(.select2-item)'))) {
            node.classList.add('show-composite-scope');
        }

        // Also apply to any child cards
        const childCards = node.querySelectorAll('.issue-card:not(.select2-item), .solution-card:not(.select2-item)');
        childCards.forEach(child => {
            child.classList.add('show-composite-scope');
        });
    }

   
});