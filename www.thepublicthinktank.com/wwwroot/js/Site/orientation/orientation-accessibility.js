let deactivateFocusTrap = null;

function orientationIntroAccessibility() {
    deactivateFocusTrap?.(); // remove old trap if active
    const startOrientationBtn = document.querySelector("#start-orientation-btn");
    startOrientationBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    deactivateFocusTrap = focusTrap_BySelectors([modal]);
}

function IssueSolutionCard() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const cardTarget = document.querySelector(".issue-card:nth-child(2), .solution-card:nth-child(2)");
    deactivateFocusTrap = focusTrap_BySelectors([modal, cardTarget]);
}

function VoteDialContainer() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const dial = document.querySelector(".dial");
    deactivateFocusTrap = focusTrap_BySelectors([modal, dial]);
}

function VoteDial() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const dial = document.querySelector(".dial .vote-dial-toggle");
    deactivateFocusTrap = focusTrap_BySelectors([modal, dial]);
}

function VoteCount() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const dial = document.querySelector(".dial .vote-count");
    deactivateFocusTrap = focusTrap_BySelectors([modal, dial]);
}

function ScopeRibbon() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const ribbon = document.querySelector(".ribbon");
    const expandToggle = document.querySelector(".card-expand-toggle");
    deactivateFocusTrap = focusTrap_BySelectors([modal, ribbon, expandToggle]);
}


function QuickLinkInfoCounts() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const links = document.querySelector(".issue-card-stat-icons, .solution-card-stat-icons");
    deactivateFocusTrap = focusTrap_BySelectors([modal, links]);
}

function breadcrumbAccessibility() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const breacdrumb = document.querySelector("nav[aria-label=breadcrumb]");
    deactivateFocusTrap = focusTrap_BySelectors([modal, breacdrumb]);
}

function leftSidebarToggle() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const sidebarToggle = document.querySelector(".left-sidebar-toggle");
    deactivateFocusTrap = focusTrap_BySelectors([modal, sidebarToggle]);
}

function sidebarViews() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const sidebarViews = document.querySelector(".view-mode-toggle");
    deactivateFocusTrap = focusTrap_BySelectors([modal, sidebarViews]);
}

function contentFilter() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const leftSidebar = document.querySelector("#left-sidebar-container");
    deactivateFocusTrap = focusTrap_BySelectors([modal, leftSidebar]);
}

function MidModalTrapOnly() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#step-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    deactivateFocusTrap = focusTrap_BySelectors([modal]);
}

function createIssuesAndSolutions1() {
    deactivateFocusTrap?.();
    const createIssueViaCreateButton = document.querySelector("#create-button-custom-orientation-step");
    createIssueViaCreateButton.focus();
    const modal = document.querySelector("#orientation-modal");
    deactivateFocusTrap = focusTrap_BySelectors([modal]);
}

function createIssuesAndSolutions2() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#workflow-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const createBtn = document.querySelector("#createDropdown")
    deactivateFocusTrap = focusTrap_BySelectors([modal, createBtn]);
}

function createBtnDropdown() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#go-back-to-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const createBtnDropdown = document.querySelector(".create-content-dropdown")
    deactivateFocusTrap = focusTrap_BySelectors([modal, createBtnDropdown]);
}

function focusGoBackToOrientationBtn() {
    const stepForwardBtn = document.querySelector("#go-back-to-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    deactivateFocusTrap = focusTrap_BySelectors([modal]);
}

function viewContentBtnStep() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#workflow-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const viewBtn = document.querySelector(".view-btn")
    const breadcrumb = document.querySelector(".breadcrumb-ribbon-custom")
    deactivateFocusTrap = focusTrap_BySelectors([modal, viewBtn, breadcrumb]);
}

function tabbarNavigation() {
    deactivateFocusTrap?.();
    const stepForwardBtn = document.querySelector("#workflow-forward-orientation-btn");
    stepForwardBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    const tabbar = document.querySelector(".sub-content-tab-bar")
    deactivateFocusTrap = focusTrap_BySelectors([modal, tabbar]);
}

function lastStep() {
    deactivateFocusTrap?.();
    const lastStepBtn = document.querySelector("#finish-orientation-btn");
    lastStepBtn.focus();
    const modal = document.querySelector("#orientation-modal");
    deactivateFocusTrap = focusTrap_BySelectors([modal]);
}

function userVotedDuringOrientationAccessibility() {
    deactivateFocusTrap?.();

    // There is not a good way to focus on elements out side the confirmVoteModal. BS sets its own focus trap on modals
    // The best option would be to not use BS modals for this.

    //const goBackButton = document.querySelector("#go-back-to-orientation-btn");
    //goBackButton.focus();
    //const modal = document.querySelector("#orientation-modal");
    //const voteConfirmModal = document.getElementById("confirmVoteModal");
    //deactivateFocusTrap = focusTrap_BySelectors([modal, voteConfirmModal]);
}

function focusTrap_BySelectors(targets) {
    if (!Array.isArray(targets)) {
        console.warn("focusTrap_BySelectors: expects an array of elements.");
        return () => { };
    }

    const containers = targets.filter(el => el instanceof Element);
    if (containers.length === 0) return () => { };

    const getFocusable = container => {
        const descendants = Array.from(
            container.querySelectorAll(
                'a[href], button:not([disabled]), textarea:not([disabled]), ' +
                'input:not([disabled]):not([tabindex="-1"]), select:not([disabled]):not([tabindex="-1"]), ' +
                '[tabindex]:not([tabindex="-1"])'
            )
        );
        // Include container itself if it can be focused (e.g. has tabindex)
        if (
            container.tabIndex >= 0 &&
            (container.offsetParent !== null || window.getComputedStyle(container).position === 'fixed')
        ) {
            descendants.unshift(container);
        }

        return descendants.filter(el =>
            el.offsetParent !== null || window.getComputedStyle(el).position === 'fixed'
        );
    };

    const getCombinedFocusable = () => {
        return containers.flatMap(getFocusable);
    };

    const onKeydown = e => {
        if (e.key !== "Tab") return;

        const focusableEls = getCombinedFocusable();
        if (focusableEls.length === 0) return;

        const activeEl = document.activeElement;
        const currentIndex = focusableEls.indexOf(activeEl);

        // Only run if focus is currently within one of the containers
        const focusInside = containers.some(c => c.contains(activeEl));
        if (!focusInside && currentIndex === -1) return;

        e.preventDefault(); // stop browser from tabbing outside our group

        let nextIndex;
        if (e.shiftKey) {
            nextIndex = currentIndex <= 0
                ? focusableEls.length - 1
                : currentIndex - 1;
        } else {
            nextIndex = currentIndex === focusableEls.length - 1
                ? 0
                : currentIndex + 1;
        }

        focusableEls[nextIndex].focus();
    };

    document.addEventListener("keydown", onKeydown);

    // 👇 Return a cleanup function to disable this trap
    return () => {
        document.removeEventListener("keydown", onKeydown);
    };
}
