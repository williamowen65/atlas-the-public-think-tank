function dynamicDropdownContent({
    defaultTab,
    parentContentType,
    issueId,
    parentIssueId,
    parentSolutionId
}) {
    const dropdownMenu = document.getElementById('dynamic-actions');


    let parentPostTabContent;
    if (parentIssueId) {
        parentPostTabContent = `<li><a class="dropdown-item" href="/issue/${parentIssueId}">Go to Parent Issue</a></li>`
    }
    if (parentSolutionId) {
        parentPostTabContent = `<li><a class="dropdown-item" href="/solution/${parentSolutionId}">Go to Parent Solution</a></li>`
    }

    // Menu content templates for each tab
    const menuTemplates = {
        'solutions-tab': `
                <li><a class="dropdown-item" href="/create-solution?parentIssueID=${issueId}">Create Solution</a></li>
            `,
        'comments-tab': `
                <li><a class="dropdown-item" href="#">Create Discussion</a></li>
            `,
        'sub-issues-tab': `
                <li><a class="dropdown-item" href="/create-issue?parent${parentContentType}ID=${issueId}">Create Sub-Issue</a></li>
            `,
        'parent-post-tab': parentPostTabContent
    };

    // Function to update dropdown content based on active tab
    // Make this function available globally
    window.updateDropdownContent = function (activeTabId) {
        if (dropdownMenu && menuTemplates[activeTabId]) {
            dropdownMenu.innerHTML = menuTemplates[activeTabId];
        }
    }

    // Initialize with default tab content
    updateDropdownContent(defaultTab);

    // Add event listeners to all tabs
    const tabs = document.querySelectorAll('[data-bs-toggle="tab"]');
    tabs.forEach(tab => {
        tab.addEventListener('shown.bs.tab', function (event) {
            updateDropdownContent(event.target.id);
        });
    });

} 