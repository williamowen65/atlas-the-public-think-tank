document.addEventListener('DOMContentLoaded', async function () {
    // Load sidebar content via AJAX
    await loadSidebarContent();

    setupViewToggle()

    document.documentElement.classList.remove('sidebar-initial-closed');
    // Get the sidebar toggle button and sidebar
    const sidebarToggle = document.querySelector('.left-sidebar-toggle');
    const sidebar = document.getElementById('left-sidebar-container');
    const mainContent = document.querySelector('main');
    const footer = document.querySelector('footer');

    if (sidebarToggle && sidebar) {
        // Initialize sidebar state based on screen size
        const isMobile = window.innerWidth <= 768;
        if (isMobile) {
            // On mobile, sidebar starts closed
            sidebar.classList.remove('sidebar-open');
        } else {
            // On desktop, check if we have a saved preference
            const sidebarState = localStorage.getItem('sidebarState');
            if (sidebarState === 'closed') {
                sidebar.classList.add('sidebar-closed');
                sidebarToggle.classList.add('sidebar-closed');
                if (mainContent) mainContent.classList.add('sidebar-closed');
                if (footer) footer.classList.add('sidebar-closed');
            }
        }

        // Function to update toggle icon
        function updateToggleIcon() {
            const toggleIcon = sidebarToggle.querySelector('i');
            if (!toggleIcon) return;

            const isMobile = window.innerWidth <= 768;
            const isSidebarVisible = isMobile
                ? sidebar.classList.contains('sidebar-open')
                : !sidebar.classList.contains('sidebar-closed');

            if (isSidebarVisible) {
                toggleIcon.classList.remove('fa-bars');
                toggleIcon.classList.add('fa-times');
            } else {
                toggleIcon.classList.remove('fa-times');
                toggleIcon.classList.add('fa-bars');
            }
        }

        // Initialize icon state
        updateToggleIcon();

        // Add click event listener to toggle sidebar
        sidebarToggle.addEventListener('click', function () {
            const isMobile = window.innerWidth <= 768;

            if (isMobile) {
                // Mobile behavior - toggle 'sidebar-open' class
                sidebar.classList.toggle('sidebar-open');
                document.body.classList.toggle('sidebar-open');
            } else {
                // Desktop behavior - toggle 'sidebar-closed' class
                sidebar.classList.toggle('sidebar-closed');
                sidebarToggle.classList.toggle('sidebar-closed');
                if (mainContent) mainContent.classList.toggle('sidebar-closed');
                if (footer) footer.classList.toggle('sidebar-closed');

                // Save preference to localStorage
                const isClosed = sidebar.classList.contains('sidebar-closed');
                localStorage.setItem('sidebarState', isClosed ? 'closed' : 'open');
            }

            // Update the toggle icon
            updateToggleIcon();
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function (event) {
            const isMobile = window.innerWidth <= 768;
            if (isMobile &&
                sidebar.classList.contains('sidebar-open') &&
                !sidebar.contains(event.target) &&
                !sidebarToggle.contains(event.target)) {
                sidebar.classList.remove('sidebar-open');
                document.body.classList.remove('sidebar-open');
                updateToggleIcon();
            }
        });

        // Handle window resize
        window.addEventListener('resize', function () {
            const isMobile = window.innerWidth <= 768;
            if (isMobile) {
                // Switching to mobile - remove desktop classes, use mobile classes
                sidebar.classList.remove('sidebar-closed');
                sidebarToggle.classList.remove('sidebar-closed');
                if (mainContent) mainContent.classList.remove('sidebar-closed');
                if (footer) footer.classList.remove('sidebar-closed');

                // Reset mobile sidebar state
                sidebar.classList.remove('sidebar-open');
                document.body.classList.remove('sidebar-open');
            } else {
                // Switching to desktop - remove mobile classes
                sidebar.classList.remove('sidebar-open');
                document.body.classList.remove('sidebar-open');

                // Apply desktop state from saved preference
                const sidebarState = localStorage.getItem('sidebarState');
                if (sidebarState === 'closed') {
                    sidebar.classList.add('sidebar-closed');
                    sidebarToggle.classList.add('sidebar-closed');
                    if (mainContent) mainContent.classList.add('sidebar-closed');
                    if (footer) footer.classList.add('sidebar-closed');
                } else {
                    sidebar.classList.remove('sidebar-closed');
                    sidebarToggle.classList.remove('sidebar-closed');
                    if (mainContent) mainContent.classList.remove('sidebar-closed');
                    if (footer) footer.classList.remove('sidebar-closed');
                }
            }

            // Update the toggle icon for current state
            updateToggleIcon();
        });
    }
});



// Function to load sidebar content via AJAX
// Function to load sidebar content via AJAX
async function loadSidebarContent() {
    const sidebarContainer = document.getElementById('left-sidebar-container');

    if (!sidebarContainer) {
        console.log("Left-sidebar-container not found");
        return Promise.resolve(); // Return a resolved promise if container not found
    }

    try {
        console.log("About to fetch sidebar")
        const response = await fetch('/api/sidebar');

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const html = await response.text();
        sidebarContainer.innerHTML = html;

        return Promise.resolve(); // Explicitly return a resolved promise
    } catch (error) {
        console.error('Error loading sidebar:', error);
        sidebarContainer.innerHTML = '<div class="alert alert-danger">Failed to load sidebar content</div>';
        return Promise.reject(error); // Return a rejected promise on error
    }
}


function setupViewToggle() {

    const lens = document.getElementById("view-mode-toggle-lens");
    const treeToggle = document.getElementById("tree");
    const infoToggle = document.getElementById("info");

    // Function to move the lens
    function moveLens() {
        if (treeToggle.checked) {
            lens.style.transform = "translateX(0)"; // Move to the left
        } else if (infoToggle.checked) {
            lens.style.transform = "translateX(100%)"; // Move to the right
        }
    }

    // Attach event listeners to the radio buttons
    treeToggle.addEventListener("change", moveLens);
    infoToggle.addEventListener("change", moveLens);

    // Initialize lens position on page load
    moveLens();
}