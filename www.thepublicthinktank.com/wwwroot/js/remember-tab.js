document.addEventListener("DOMContentLoaded", function () {
    // Only run on pages with tab system
    const tabContainer = document.getElementById('nav-tab');
    if (!tabContainer) return;

    // Use a global storage key for all issues
    const storageKey = `lastActiveTab-global`;

    // Get all tab buttons
    const tabButtons = document.querySelectorAll('[data-bs-toggle="tab"]');

    // Restore the last active tab
    const lastActiveTabId = localStorage.getItem(storageKey);
    if (lastActiveTabId) {
        const lastTab = document.getElementById(lastActiveTabId);
        if (lastTab) {
            // Create a new bootstrap tab instance and show it
            const tab = new bootstrap.Tab(lastTab);
            tab.show();

            // Also trigger our reinitializeVoteDials function if it exists
            if (typeof reinitializeVoteDials === 'function') {
                const targetId = lastTab.getAttribute('data-bs-target').replace('#', '');
                setTimeout(() => reinitializeVoteDials(targetId), 100);
            }

            // Update the actions dropdown if it exists
            if (typeof dynamicDropdownContent === 'function') {
                dynamicDropdownContent();
            }
        }
    }

    // Add event listeners to save the active tab when changed
    tabButtons.forEach(tabButton => {
        tabButton.addEventListener('shown.bs.tab', function (event) {
            localStorage.setItem(storageKey, event.target.id);
        });
    });
});
