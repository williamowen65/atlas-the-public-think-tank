/**
 * JavaScript file for the left-side-bar
 * 
 * This fetches the sidebar content - after from the main page load event.
 * This sets logic to open and close the sidebar
 * Sets logic to make the sidebar float on top for mobile but toggling a class.
 * 
 * This file also manages the toggles for switching between sidebar views
 */

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
    const filterToggle = document.getElementById("filter");
    
    // Get all headings and content sections
    const treeHeading = document.getElementById("tree-heading");
    const infoHeading = document.getElementById("info-heading");
    const filterHeading = document.getElementById("filter-heading");
    
    const categoriesContent = document.getElementById("categoriesContainer");
    const pageInfoContent = document.getElementById("page-info");
    const contentFilterContent = document.getElementById("content-filter");
    
    // Function to update active content based on selected view mode
    function updateActiveContent(viewMode) {
        // Remove active class from all headings and content
        [treeHeading, infoHeading, filterHeading].forEach(el => {
            if (el) el.classList.remove('active');
        });
        
        [categoriesContent, pageInfoContent, contentFilterContent].forEach(el => {
            if (el) el.classList.remove('active');
        });
        
        // Add active class to selected heading and content
        switch (viewMode) {
            case 'tree':
                if (treeHeading) treeHeading.classList.add('active');
                if (categoriesContent) categoriesContent.classList.add('active');
                break;
            case 'info':
                if (infoHeading) infoHeading.classList.add('active');
                if (pageInfoContent) pageInfoContent.classList.add('active');
                break;
            case 'filter':
                if (filterHeading) filterHeading.classList.add('active');
                if (contentFilterContent) contentFilterContent.classList.add('active');
                break;
        }
    }

    // Function to move the lens and update active content
    function moveLens() {
        if (treeToggle.checked) {
            lens.style.transform = "translateX(0)";
            localStorage.setItem('sidebarViewMode', 'tree');
            updateActiveContent('tree');
        } else if (infoToggle.checked) {
            lens.style.transform = "translateX(100%)";
            localStorage.setItem('sidebarViewMode', 'info');
            updateActiveContent('info');
        } else if (filterToggle.checked) {
            lens.style.transform = "translateX(200%)";
            localStorage.setItem('sidebarViewMode', 'filter');
            updateActiveContent('filter');
        }
    }

    // Attach event listeners to the radio buttons
    if (treeToggle) treeToggle.addEventListener("change", moveLens);
    if (infoToggle) infoToggle.addEventListener("change", moveLens);
    if (filterToggle) filterToggle.addEventListener("change", moveLens);

    // Load saved view mode from localStorage
    const savedViewMode = localStorage.getItem('sidebarViewMode');
    if (savedViewMode) {
        switch (savedViewMode) {
            case 'tree':
                if (treeToggle) treeToggle.checked = true;
                break;
            case 'info':
                if (infoToggle) infoToggle.checked = true;
                break;
            case 'filter':
                if (filterToggle) filterToggle.checked = true;
                break;
            default:
                if (treeToggle) treeToggle.checked = true;  // Default fallback
        }
    }

    // Initialize lens position and content based on active toggle
    moveLens();
}