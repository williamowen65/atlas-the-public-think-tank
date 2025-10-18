document.addEventListener("DOMContentLoaded", () => {

    // Shortcuts are used to get focus to somewhere in the web app
    keyboardShortcut_alt_a()
    keyboardShortcut_alt_m()
    keyboardShortcut_alt_h()
    keyboardShortcut_alt_s()
    keyboardShortcut_alt_b()
    keyboardShortcut_alt_u()

    // Focus Traps are used to keep focus locked in an area
    focusTrap_BySelector('#accessibility-tab')
    arrowKeyFocusNavigation("#accessibility-tab button:visible")
    focusTrap_BySelector('body') // <-- This doesn't seem to work because the browser still gets focus after the last elements

    // These are all actions in the accessibility table
    userSelectsGoTo()
    userSelectsProvideAccessibilityFeedback() // Opens form
    userSelectsExitButton()
    userSelectsGoBackBtn()
    userSelectsGoToMainSection()
    userSelectsGoToHeader()
    userSelectsGoToSidebar()
    userSelectsGoToFooter()
    userSelectsGoToUtilityDock()

    // These are general accesibility logic 
    keyboardShortcut_alt_down_and_up_for_content_cards() // A quick way to move focus between content cards (issues/solutions)
    addEnterSupportToLabels() // Clicking focusable elements with enter
    userSelectsProvideGeneralFeedback() // On open of a form, focus input area
})


function arrowKeyFocusNavigation(selector) {
    $(selector).toArray().forEach((el, idx, arr) => {
        el.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                arr[(idx + 1) % arr.length].focus();
            }
            if (e.key === 'ArrowUp') {
                e.preventDefault();
                arr[(idx - 1 + arr.length) % arr.length].focus();
            }
        });
    });
}
function addEnterSupportToLabels() {
    document.querySelectorAll('label[role="button"][tabindex="0"]').forEach(label => {
        label.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                const inputId = label.getAttribute('for');
                const input = document.getElementById(inputId);
                if (input && !input.disabled) {
                    input.checked = true;
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                }
                e.preventDefault();
            }
        });
    });
    document.querySelectorAll('div[role="button"][tabindex="0"]').forEach(element => {
        element.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                element.click();
                e.preventDefault();
            }
        });
    });
}

function keyboardShortcut_alt_u() {
    document.addEventListener('keydown', (e) => {
        if (e.altKey && e.key === 'u') { // alt + u
            focusUtilityDock()
        }
    });
}

function keyboardShortcut_alt_b() {
    document.addEventListener('keydown', (e) => {
        if (e.altKey && e.key === 'b') { // alt + b
            focusFooter()
        }
    });
}

function keyboardShortcut_alt_s() {
    document.addEventListener('keydown', (e) => {
        if (e.altKey && e.key === 's') { // alt + s
            focusSidebar()
        }
    });
}

function keyboardShortcut_alt_h() {
    document.addEventListener('keydown', (e) => {
        if (e.altKey && e.key === 'h') { // alt + h
            focusHeader()
        }
    });
}

function keyboardShortcut_alt_m() {
    document.addEventListener('keydown', (e) => {
        if (e.altKey && e.key === 'm') { // alt + m
            goToMainContent()
        }
    });
}

function keyboardShortcut_alt_down_and_up_for_content_cards() {
    document.addEventListener('keydown', (e) => {
        if (e.altKey && e.key === 'ArrowDown') {
            const closestContentCard = document.activeElement.closest(".issue-card, .solution-card")
            if (closestContentCard) {
                // focus the next card
                closestContentCard.nextElementSibling.focus()
            }
        }
        if (e.altKey && e.key === 'ArrowUp') {
            const closestContentCard = document.activeElement.closest(".issue-card, .solution-card")
            if (closestContentCard) {
                // focus the next card
                closestContentCard.previousElementSibling.focus()
            }
        }
    });
}

function userSelectsGoToUtilityDock() {
    const goToUtilityDockButton = document.querySelector("#go-to-utility-dock")
    goToUtilityDockButton.addEventListener("click", () => {
        focusUtilityDock()
    })
}
function focusUtilityDock() {
    const utilityDock = document.querySelector("#utility-dock")
    utilityDock.focus()
}

function userSelectsGoToFooter() {
    const goToFooterButton = document.querySelector("#go-to-footer")
    goToFooterButton.addEventListener("click", () => {
        focusFooter()
    })
}

function focusFooter() {
    const footer = document.querySelector("footer.footer")
    footer.focus()
}

function userSelectsGoToHeader() {
    const goToHeaderButton = document.querySelector("#go-to-header")
    goToHeaderButton.addEventListener("click", () => {
        focusHeader()
    })
}

function focusHeader() {
    const header = document.querySelector("header.main-header")
    header.focus()
}

function userSelectsGoToSidebar() {
    const goToSidebarButton = document.querySelector("#go-to-sidebar")
    goToSidebarButton.addEventListener("click", () => {
        focusSidebar()
    })
}

function focusSidebar() {
    const sidebar = document.querySelector("#left-sidebar-container")
    sidebar.focus()
    const isMobile = window.innerWidth <= 768;
    //Todo: also make sure sidebar is open.
    if (isMobile) {
        const sidebarToggle = document.querySelector(".left-sidebar-toggle")
        sidebarToggle.click();
    } else {
        const leftSideBarToggleShowsClosed = document.querySelector(".left-sidebar-toggle.sidebar-closed")
        if (leftSideBarToggleShowsClosed) {
            leftSideBarToggleShowsClosed.click();
        }
    }

}

function userSelectsGoToMainSection() {
    const goToMainSectionBtn = document.querySelector("#go-to-main-section")
    goToMainSectionBtn.addEventListener("click", () => {
        goToMainContent()
    })
}

function goToMainContent() {
    let mainSection;
    const isOnHomePage = location.pathname == "/"
    if (isOnHomePage) {
        mainSection = document.querySelector("#main-content .issue-card, #main-content .solution-card")
    }
    const isOnContentPage = location.pathname.match(new RegExp(/(\/issue|\/solution)/))
    if (isOnContentPage) {
        mainSection = document.querySelector(".issue-card, .solution-card")
    }

    if (mainSection) {
        mainSection.focus()
    } else {
        console.warn("Accessibility error: Main section was not known for focus")
    }
}



function userSelectsGoBackBtn() {
    const goBackButton = document.querySelector("#go-back-from-go-to-menu")

    goBackButton.addEventListener("click", () => {
        const accessibilityTab = document.querySelector("#accessibility-tab")
        accessibilityTab.focus()
        const mainMenu = document.querySelector(".accessibility-menu")
        mainMenu.classList.remove("d-none")
        const goToMenu = document.querySelector(".go-to-menu")
        goToMenu.classList.add("d-none")
        focusTrap_BySelector('#accessibility-tab')
        arrowKeyFocusNavigation("#accessibility-tab button:visible")
    })
}

function userSelectsExitButton() {
    const exitBtn = document.querySelector("#exit-accessibility-tab-button")
    exitBtn.addEventListener("click", () => {
        const accessibilityTab = document.querySelector("#accessibility-tab")
        accessibilityTab.focus()
        accessibilityTab.blur();
    })
}

/**
 * This form uses bootstrap attributes to open the modal (bs-target)
 * InBoundEmail.js is used for the actual feedback form
 */
function userSelectsProvideAccessibilityFeedback() {
    // The html element use bootstrap attributes to open the modal
    const accessibilityFeedbackModal = document.querySelector("#accessibility-feedback-modal")
    accessibilityFeedbackModal.addEventListener("shown.bs.modal", () => {
        accessibilityFeedbackModal.querySelector("#Message").focus();
    })
}


/**
 * This form is opened via JS in InBoundEmail.js
 */
function userSelectsProvideGeneralFeedback() {
    const generalFeedbackModal = document.querySelector("#general-feedback-modal");
    generalFeedbackModal.addEventListener("shown.bs.modal", () => {
        generalFeedbackModal.querySelector("#Message").focus();
    })
}


function userSelectsGoTo() {
    const goToButton = document.querySelector("#go-to")

    goToButton.addEventListener("click", () => {
        const accessibilityTab = document.querySelector("#accessibility-tab")
        accessibilityTab.focus()
        const mainMenu = document.querySelector(".accessibility-menu")
        mainMenu.classList.add("d-none")
        const goToMenu = document.querySelector(".go-to-menu")
        goToMenu.classList.remove("d-none")
        focusTrap_BySelector('#accessibility-tab')
        arrowKeyFocusNavigation("#accessibility-tab button:visible")
    })
}



function keyboardShortcut_alt_a() {

    document.addEventListener('keydown', (e) => {
        if (e.altKey && e.key === 'a') { // Alt + A
            document.getElementById('accessibility-tab').focus();
        }
    });
}

function focusTrap_BySelector(target) {
    const container = document.querySelector(target);

    container.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            // Refresh focusable elements each time
            const focusableEls = $(container).find('button:visible, [tabindex]:visible, a:visible').toArray();
            let firstEl = focusableEls[0];
            let lastEl = focusableEls[focusableEls.length - 1];

            if (e.shiftKey && (document.activeElement === firstEl || document.activeElement == container)) {
                e.preventDefault();
                lastEl.focus();
            } else if (!e.shiftKey && (document.activeElement === lastEl || document.activeElement == container)) {
                e.preventDefault();
                firstEl.focus();
            }
        }
    });
}