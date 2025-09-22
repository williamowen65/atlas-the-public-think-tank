
/**
 * JavaScript file for issue/solution cards
 * 
 * This initializes the vote dial so that 
 * the lens is on the correct value, 
 * the lens follows the desired value, 
 * the dial can cast votes (Debounced - Wait until the dial has been still for a second before trying to vote)
 * 
 * Note: This file makes use of the IntersectionObserver API to track when a value changes during scroll with in the dial.
 * Note: This file also uses the MutationObserver API to track when issue-cards are added to the DOM
 * Note: This file is added to the <head> element, not the <body> element
 * 
 *
 */

function initializeCard(cardId) {
    const card = document.querySelector(`.card[id="${cardId}"]`)

    const isVersionHistoryModal = card.closest("#versionControlModal");

    if (!isVersionHistoryModal) {
        initializeVoteDial(cardId)
         // TBD - More initializations are possible
    }
    initializeCompositeScopeRibbonListener(cardId)

    //card.classList.add("initialized")
}


// Delgated listener for some card click events
document.addEventListener("click", e => {
    if (e.target.closest(".card-expand-toggle, .card-minimize-toggle")) {
        const card = e.target.closest(".card")
        const toggles = card.querySelectorAll(".card-expand-toggle, .card-minimize-toggle")
        Array.from(toggles).forEach(t => {
            t.classList.toggle("d-none")
        })
        card.classList.toggle("expanded")
        const truncatedTextElement = card.querySelector(".text-collapsible-target")
        if (truncatedTextElement.classList.contains("truncate-multiline")) {
            truncatedTextElement.classList.remove("truncate-multiline");
        } else {
            truncatedTextElement.classList.add("truncate-multiline");
        }

        const isMinimizetoggle = Boolean(e.target.closest(".card-minimize-toggle"))
        //const isExpandtoggle = Boolean(e.target.closest(".card-expand-toggle"))
        //if (isExpandtoggle) {
            // Move scroll position to top of card (plus header height (var(--header-height)))
            const headerHeight = parseInt(getComputedStyle(document.documentElement).getPropertyValue('--header-height')) || 0;
            const cardTop = card.getBoundingClientRect().top + window.scrollY;
            window.scrollTo({
                top: cardTop - headerHeight,
                behavior: isMinimizetoggle ? 'instant' :'smooth'
            });
        //}

        //if (isMinimizetoggle) {
        //    // Retain the scroll position around the clicked element.
        //    // Get card's position relative to viewport before minimizing
        //    const prevRect = e.target.getBoundingClientRect();
        //    const prevTop = prevRect.top;

        //    // Use setTimeout to wait for DOM changes (minimize toggle) to take effect
        //    setTimeout(() => {
        //        const newRect = e.target.getBoundingClientRect();
        //        const newTop = newRect.top;
        //        // Calculate the difference and adjust scroll
        //        const scrollDiff = newTop - prevTop;
        //        window.scrollBy({ top: scrollDiff, behavior: 'instant' });
        //    }, 0);
        //}
    }

    setupQuickTabLinks(e)
})


/**
 * In Issue and Solution cards, there are icons for comments, sub-issues, and solutions.
 * These icons are links to the content page with the tab selected automatically
 * @param {Event} e
 * @returns void
 */
function setupQuickTabLinks(e) {


    if (e.target.closest(".stat-icon-link")) {
        // Set the lastActiveTab-global local storage
        // #sub-issue-tab, #solutions-tab, #comments-tab
        const statIcon = e.target.closest(".stat-icon-link");
        let tabId = null;

        // Determine which tab to activate based on the clicked icon
        if (statIcon.classList.contains("go-to-content-item-solution-tab")) {
            tabId = "solutions-tab";
        } else if (statIcon.classList.contains("go-to-content-item-sub-issue-tab")) {
            tabId = "sub-issues-tab";
        } else if (statIcon.classList.contains("go-to-content-item-comment-tab")) {
            tabId = "comments-tab";
        } else if (statIcon.classList.contains("show-version-history")) {
            fetchVersionHistory(e)
            return;
        }

        if (tabId) {
            // Store the tab ID in localStorage
            localStorage.setItem("lastActiveTab-global", tabId);

            // Find the content page URL to navigate to
            const card = statIcon.closest(".card");
            if (card) {
                const viewButton = card.querySelector(".view-btn")
                viewButton.click()
            }
        }
    }
}

/**
 * Clicking the Version history icon should popup a modal of version changes
 * @param {Event} e
 */
function fetchVersionHistory(e) {
    const contentCard = e.target.closest(".card")
    const contentType = contentCard.getAttribute('data-content-type');
    const contentId = contentCard.getAttribute("id")

    fetch(`/${contentType}-version-history?${contentType}Id=${contentId}`)
        .then(res => res.json())
        .then(res => {
            const htmlString = res.content;
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = htmlString;

            // Process version history markup BEFORE appending to body
            isolateAndDisableVersionHistoryDials(tempDiv);

            // Append children to body
            while (tempDiv.firstChild) {
                document.body.appendChild(tempDiv.firstChild);
            }

            // Show the modal
            const modalElement = document.getElementById('versionControlModal');
            if (modalElement && window.bootstrap && bootstrap.Modal) {
                const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
                modal.show();

                // Add event listener to remove modal from DOM on close
                modalElement.addEventListener('hidden.bs.modal', function handleModalHidden() {
                    modalElement.removeEventListener('hidden.bs.modal', handleModalHidden);
                    modalElement.parentNode && modalElement.parentNode.removeChild(modalElement);
                });

            } else {
                console.error('Version history modal element or Bootstrap JS not found.');
            }
        })
        .catch(err => {
            console.log(err)
        })
}


/**
  * Prepares the version history modal content to NOT conflict with the main content.
  * Prevents duplicate IDs on the page related to dom events
  * @param {DOMElement} root
  * @returns void
  */
function isolateAndDisableVersionHistoryDials(root) {
    // Inject style once
    if (!document.getElementById('vh-dial-style')) {
        const style = document.createElement('style');
        style.id = 'vh-dial-style';
        style.textContent = `
            .vh-dial-disabled-wrapper { position:relative; }
            .vh-dial-disabled-overlay {
                position:absolute;
                inset:0;
                background:rgba(0,0,0,0.55);
                display:flex;
                align-items:center;
                justify-content:center;
                flex-direction:column;
                color:#fff;
                font-size:.9rem;
                font-weight:600;
                text-align:center;
                z-index:5;
                padding:.5rem;
                border-radius:4px;
            }
            .vh-dial-disabled-overlay span.small {
                font-weight:400;
                font-size:.75rem;
                opacity:.85;
            }
            .vh-dial-disabled-wrapper .toggle-option,
            .vh-dial-disabled-wrapper input[type=radio] {
                pointer-events:none !important;
            }
            .vh-dial-disabled-wrapper.user-voted { /* just in case */
                pointer-events:none;
            }
        `;
        document.head.appendChild(style);
    }

    // Find cards inside the modal
    const modal = root.querySelector('#versionControlModal');
    if (!modal) return;

    // Remove author-content-alerts
    const authorAlerts = Array.from(modal.querySelectorAll(".author-content-alert"))
    authorAlerts.forEach(alert => {
        alert.remove()
    })


    const cards = modal.querySelectorAll('.card[data-content-type]');
    

    cards.forEach((card, idx) => {
        if (!card.id) return;

        // Remove default bottom margin
        card.classList.remove("mb-3")
        // Remove top corner border radius
        card.classList.add("rounded-top-0")
      

        const originalCardId = card.id;
        card.setAttribute('data-original-card-id', originalCardId);
        const newCardId = `vh-${originalCardId}-${idx}`;
        card.id = newCardId;

        // Vote dial container pattern: vote-toggle-container-{originalId}
        const voteContainer = card.querySelector(`#vote-toggle-container-${originalCardId}`);
        if (!voteContainer) return;

        const parentWrapper = voteContainer.parentElement;
        if (parentWrapper && !parentWrapper.classList.contains('vh-dial-disabled-wrapper')) {
            parentWrapper.classList.add('vh-dial-disabled-wrapper');
        }

        // Rewrite container id
        const newContainerId = `vh-vote-toggle-container-${originalCardId}-${idx}`;
        voteContainer.id = newContainerId;

        // Collect radios BEFORE renaming
        const radios = voteContainer.querySelectorAll('input[type="radio"]');

        // Determine old radio group name (assume consistent)
        let oldGroupName = null;
        if (radios.length) {
            oldGroupName = radios[0].name; // e.g., vote-dial-123
        }

        // New group name
        const newGroupName = oldGroupName ? `vh-${oldGroupName}-${idx}` : null;

        // Rewrite radios + labels
        radios.forEach((r, rIdx) => {
            const oldId = r.id;
            const newId = `vh-${oldId}-${idx}`;
            r.id = newId;
            if (newGroupName) r.name = newGroupName;
            r.disabled = true;
            r.setAttribute('aria-disabled', 'true');
        });

        voteContainer.querySelectorAll('label[for]').forEach(label => {
            const oldFor = label.getAttribute('for');
            if (oldFor) {
                label.setAttribute('for', `vh-${oldFor}-${idx}`);
            }
        });

        // Add overlay banner (only once per container)
        if (!voteContainer.querySelector('.vh-dial-disabled-overlay')) {
            const overlay = document.createElement('div');
            overlay.className = 'vh-dial-disabled-overlay';
            overlay.innerHTML = `
                <div>
                    Disabled
                </div>
            `;
            parentWrapper.appendChild(overlay);
        }
    });
}


function getSelectedRadioValue(dialId, container) {
    const checkedRadio = container.querySelector(`input[name="${dialId}"][checked]`);
    return checkedRadio ? checkedRadio.value : null;
}

// DIAL JS

function getDialElements(issueId) {

    //if (!issueId) {
    //    console.error("IssueId not passed to getDialElements")
    //}

    //console.trace("getDialElements");

    const containerId = `vote-toggle-container-${issueId}`;
    const container = document.getElementById(containerId);
    const card = container.closest(".card")
    const contentType = card.getAttribute("data-content-type")

    if (!container) {
        console.error(`Container not found for issue ID: ${issueId}`);
        return { container: null, dialId: null, options: [], radios: [] };
    }

    const dialId = `vote-dial-${issueId}`;

    // Scope these queries to within the container only
    const options = Array.from(container.querySelectorAll('.toggle-option'));

    // Scope radio queries to within the container or to the specific name
    const radios = Array.from(container.querySelectorAll(`input[name="${dialId}"]`));

    // Returns the value of the selected radio button for a given dialId, or null if none selected
  
   


    return {
        container, dialId, options, radios, contentType
    }
}


/**
 * This is called multiple times on the same page for multiple dials
 * It knows where to add the dial to the DOM and will set all related events
 * @param {int} issueId - used to fetch the correct dial data
 * @returns void. 
 */
function initializeVoteDial(issueId) {
    //console.log("Vote dial initialized for issue ID:", issueId);
    
    // Get essential elements
    const { container, dialId, options, radios } = getDialElements(issueId);

    if (!container) {
        console.error(`Cannot initialize dial for issue ${issueId}: container not found`);
        return;
    }

    if (options.length === 0 || radios.length === 0) {
        console.error(`Cannot initialize dial for issue ${issueId}: missing options or radios`);
        return;
    }

    
    
    // Create shared state object (passed by reference)
    const state = {
        isScrolling: false,
        scrollTimeout: null,
        isResetting: false
    };
    
    // Initialize components
    const confirmVoteDebounced = createDebouncedConfirmVote(issueId);
    const observer = configureIntersectionObserverForDialVotes(container, options, state, dialId);
    
    // Set up UI and event handlers
    scrollToSelectedOption(container, dialId);
    setupScrollEvents(container, dialId, state);
    setupRadioChangeEvents(radios, confirmVoteDebounced, container, state);
    //createDialResetMethod(container, issueId, observer, dialId, options, state);


    //console.log("Dial initialized: ", issueId)
    //console.trace("Dial initialized: ", issueId)
}

/**
 * Scrolls the container to center the currently selected option
 */
function scrollToSelectedOption(container, dialId) {
    const checkedRadio = document.querySelector(`input[name="${dialId}"]:checked`);
    
    if (checkedRadio) {
        const radioId = checkedRadio.id;
        const label = document.querySelector(`label[for="${radioId}"]`);

        if (label) {
            const labelTop = label.offsetTop;
            const containerHeight = container.clientHeight;
            const labelHeight = label.clientHeight;

            container.scrollTop = labelTop - (containerHeight / 2) + (labelHeight / 2);
        }
    }
}

/**
 * Sets up scroll event handling for the vote dial
 */
function setupScrollEvents(container, dialId, state) {
    container.addEventListener('scroll', () => {
        state.isScrolling = true;
        clearTimeout(state.scrollTimeout);
        state.scrollTimeout = setTimeout(() => {
            state.isScrolling = false;
            const checkedRadio = document.querySelector(`input[name="${dialId}"]:checked`);
            if (checkedRadio) {
                const label = document.querySelector(`label[for="${checkedRadio.id}"]`);
                if (label) {
                    const labelTop = label.offsetTop;
                    const containerHeight = container.clientHeight;
                    const labelHeight = label.clientHeight;

                    container.scrollTo({
                        top: labelTop - (containerHeight / 2) + (labelHeight / 2),
                        behavior: 'smooth'
                    });
                }
            }
        }, 150);
    });
}

/**
 * Sets up radio button change event handling
 */
function setupRadioChangeEvents(radios, saveVoteDebounced, container, state) {
    radios.forEach(radio => {
        // Replace radio with a clone to remove all previous listeners
        const clone = radio.cloneNode(true);
        radio.parentNode.replaceChild(clone, radio);

        clone.addEventListener('change', function () {
            if (this.checked) {
                const value = parseInt(this.value);

                // Only save valid vote values (0-10)
                if (value >= 0 && value <= 10) {
                    saveVoteDebounced(value);
                }

                const label = document.querySelector(`label[for="${this.id}"]`);
                if (label) {
                    const labelTop = label.offsetTop;
                    const containerHeight = container.clientHeight;
                    const labelHeight = label.clientHeight;

                    state.isScrolling = false;

                    container.scrollTo({
                        top: labelTop - (containerHeight / 2) + (labelHeight / 2),
                        behavior: 'smooth'
                    });
                }
            }
        });
    });
}

const pageLoadMoment = Date.now();

function saveVote(voteValue, {
    contentId,
    container,
    dialId,
    contentType
}) {
    console.log(`Saving vote ${voteValue} for ${contentType} ${contentId}`);

    /*
    There is a known error where the voting dial accidentally
    votes on load (sometimes). 
    To prevent those from triggering a vote, there is a check. 
    Voting can only begin one second after page load
    */
    const currentMoment = Date.now();
    const timeSincePageLoad = currentMoment - pageLoadMoment;

    // Prevent votes within the first second after page load
    if (timeSincePageLoad < 1000) {
        console.log('Vote prevented - too soon after page load');
        return; // Exit the function without saving the vote
    }


    const formData = new FormData();
    formData.append(`${contentType}ID`, contentId);
    formData.append('VoteValue', voteValue);


    return fetch(`/${contentType}/Vote`, {
        method: 'POST',
        body: JSON.stringify({
            [`${contentType}ID`]: contentId,
            'VoteValue': voteValue
        }),
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(async (response) => {

            if (!response.ok) {

                const test = await response.json();

                throw new Error(test.message);
            }
            return await response.json();
        })
        .then(data => {
            console.log('Vote saved successfully:', data);
            // Optionally update the UI based on response

            // Add the user-voted class
            container.closest(".vote-dial-toggle").classList.add('user-voted');

            // update the vote average and the count
            const averageElement = document.querySelector(`#vote-average-${contentId}`);
            const countElement = document.querySelector(`#vote-count-${contentId}`);

            if (averageElement && data.average !== undefined) {
                averageElement.textContent = Number.isInteger(data.average) ? data.average.toString() : data.average.toFixed(1);
            }

            if (countElement && data.count !== undefined) {
                countElement.textContent = data.count;
            }

            // Ensure only the correct radio input has the 'checked' attribute
            // This is important for reseting the vote to the correct position
            const radioInputs = container.querySelectorAll("input[type=radio]");
            radioInputs.forEach(radio => {
                if (parseInt(radio.value) === voteValue) {
                    radio.setAttribute("checked", true);
                } else {
                    radio.removeAttribute("checked");
                }
            });

        })
 
}

/**
 * Creates a debounced function for saving votes to the server
 */
function createDebouncedConfirmVote(contentId) {
    const { container, dialId, contentType } = getDialElements(contentId);
    // Function to save vote to server with debouncing

    const contentReferences = {
        contentId,
        container,
        dialId,
        contentType
    }

    return debounce(function (voteValue) {

        contentReferences.previousExistingVote = getSelectedRadioValue(dialId, container)

        confirmVote(voteValue, contentReferences)
            .then(() => {
                console.log('Promise resolved: proceeding to saveVote');
                return saveVote(voteValue, contentReferences);
            })
            .catch((error) => {
                console.log('Promise rejected: caught in catch block', error);
                showErrorAlert(error, contentReferences);
            });
    }, 800); // 800ms debounce delay
}

function showErrorAlert(error, {
    contentId,
    container,
    dialId,
    contentType,
    previousExistingVote = 5 // This is the default for no vote cast
}) {
        console.error('Error saving vote:', error);

        // Select dial number 5
        const defaultRadio = document.querySelector(`input[name="${dialId}"][value="${previousExistingVote}"]`);

        if (defaultRadio) {
            // Add class to temporarily prevent observer
            container.classList.add('temporarily-prevent-observer');

            // Set checked state
            defaultRadio.checked = true;

            // Find the label for scrolling
            const label = document.querySelector(`label[for="${defaultRadio.id}"]`);
            if (label) {
                const labelTop = label.offsetTop;
                const containerHeight = container.clientHeight;
                const labelHeight = label.clientHeight;

                // Just scroll to position without triggering the change event
                container.scrollTo({
                    top: labelTop - (containerHeight / 2) + (labelHeight / 2),
                    behavior: 'smooth'
                });
            }

            // Remove the class after a short delay
            setTimeout(() => {

                client_TopBar_Alert({
                    type: 'warning',
                    message: `
                    <h4>Vote not cast</h4>
                    <p>${error.message}</p>
                    `,
                });

                client_CardFooter_Alert({
                    cardId: contentId,
                    type: 'plaintext',
                    message: `
                  Vote not cast - ${error.message}
                    `,
                    dismissible: false
                });
            }, 500);
            setTimeout(() => {
                container.classList.remove('temporarily-prevent-observer');
            }, 1000) // 1 second delay to allow scroll to finish
        }
}


async function confirmVote(voteValue, contentData) {
    return fetch(`/vote-request`, {
        method: 'POST',
        body: JSON.stringify({
            'ContentID': contentData.contentId,
            'ContentType': contentData.contentType,
            'VoteValue': voteValue
        }),
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(async (response) => {

            if (!response.ok) {

                const test = await response.json();

                throw new Error(test.message);
            }
            return await response.json();
        })// After receiving response.html from the server
        .then(response => {
            //console.log({ response });

           
            // 1. Add the modal HTML to the DOM
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = response.html;
            while (tempDiv.firstChild) {
                document.body.appendChild(tempDiv.firstChild);
            }

            // 2. Get the modal element
            const modalElement = document.getElementById('confirmVoteModal');
            if (modalElement && window.bootstrap && bootstrap.Modal) {
                const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
                modal.show();

                //// 3. Remove modal from DOM on close
                modalElement.addEventListener('hidden.bs.modal', function handleModalHidden() {
                    modalElement.removeEventListener('hidden.bs.modal', handleModalHidden);
                    modalElement.parentNode && modalElement.parentNode.removeChild(modalElement);
                });

                console.log("About to return promise for confirm")

            
                return new Promise((resolve, reject) => {
                    const confirmBtn = modalElement.querySelector('#confirmVoteModalConfirmBtn');
                    if (confirmBtn) {
                        confirmBtn.addEventListener('click', function () {
                            modal.hide();
                            resolve();
                        });
                    }
                    const closeBtn = modalElement.querySelector('.close-modal');
                    if (closeBtn) {
                        closeBtn.addEventListener('click', function (e) {
                            modal.hide();
                            reject(new Error('Vote confirmation was cancelled by user.'));
                        });
                    }
                });



            } else {
                console.error('Confirm vote modal element or Bootstrap JS not found.');
            }
        })
     

   

}
/**
 * Sets up intersection observer to track visible options during scrolling
 */
function configureIntersectionObserverForDialVotes(container, options, state, dialId) {
    const observer = new IntersectionObserver((entries) => {
        if (!state.isScrolling || state.isResetting || container.classList.contains('temporarily-prevent-observer')) return;

        let maxVisibility = 0;
        let mostVisibleEntry = null;

        entries.forEach(entry => {
            const visibleRatio = entry.intersectionRatio;
            if (visibleRatio > maxVisibility) {
                maxVisibility = visibleRatio;
                mostVisibleEntry = entry;
            }
        });

        if (mostVisibleEntry && maxVisibility > 0.99) {
            const label = mostVisibleEntry.target;
            const radioId = label.getAttribute('for');
            const radio = document.getElementById(radioId);

            const value = parseInt(radio.value);
            if (radio && !radio.checked && value >= 0 && value <= 10) {
                radio.checked = true;
                const event = new Event('change');
                radio.dispatchEvent(event);
            }
        }
    }, {
        root: container,
        threshold: [0, 0.25, 0.5, 0.75, 1],
        rootMargin: '-10% 0px -10% 0px'
    });

    options.forEach(option => {
        observer.observe(option);
    });

    return observer;
}

/**
 * Adds a reset method to the container that resets the dial to the default position
 */
function createDialResetMethod(container, issueId, observer, dialId, options, state) {
    container.reset = function (silent = false) {
        console.log(`Resetting vote dial for issue ${issueId}`);

        // Disable the observer temporarily
        const observerState = observer.takeRecords();
        observer.disconnect();

        // Flag to prevent vote events during reset
        state.isResetting = true;

        // Find the default radio (value 5)
        const defaultRadio = document.querySelector(`input[name="${dialId}"][value="5"]`);

        if (defaultRadio) {
            // Set checked state without firing events
            defaultRadio.checked = true;

            // Get the label for scrolling
            const label = document.querySelector(`label[for="${defaultRadio.id}"]`);
            if (label) {
                // Calculate position
                const labelTop = label.offsetTop;
                const containerHeight = container.clientHeight;
                const labelHeight = label.clientHeight;
                
                let containerToReset = container;
                
                if (silent) {
                    // Create a new container to replace the old one
                    const newContainer = container.cloneNode(true);
                    container.parentNode.replaceChild(newContainer, container);
                    containerToReset = newContainer;
                    
                    // We need to recreate all event handlers and methods on the new container
                    const confirmVoteDebounced = createDebouncedConfirmVote(issueId);
                    const { options, radios } = getDialElements(issueId);
                    // Recreate the reset method on the new container (recursive but will only happen once)
                    
                    scrollToSelectedOption(containerToReset, dialId);
                    setupScrollEvents(container, dialId, state);
                    setupRadioChangeEvents(radios, confirmVoteDebounced, container, state);
                    createDialResetMethod(containerToReset, issueId, observer, dialId, options, state);
                }
                
                // Set scroll position with or without animation
                containerToReset.scrollTo({
                    top: labelTop - (containerHeight / 2) + (labelHeight / 2),
                    behavior: 'smooth'
                });
                
                // Remove user-voted class if present
                containerToReset.classList.remove('user-voted');
                
                // Update the container reference for the rest of the function
                container = containerToReset;
            }
        } else {
            // If we couldn't find the default radio, just remove the user-voted class
            container.classList.remove('user-voted');
        }
        
        // Re-enable the observer after the reset is complete
        setTimeout(() => {
            options.forEach(option => {
                observer.observe(option);
            });
            state.isResetting = false;
            
            // Re-enable scrolling functionality by simulating a small scroll
            // This will trigger the scroll event and set isScrolling to true
            if (!silent) {
                const currentScrollTop = container.scrollTop;
                container.scrollTop = currentScrollTop + 1;
                setTimeout(() => {
                    container.scrollTop = currentScrollTop;
                }, 10);
            }
        }, 500); // Delay to ensure scrolling is complete
    };
}




/**
 * This mutation observer watches for issue-cards to be added to the dom
 * It calls logic for the voting dials to be initialized per issue card added to DOM
 * 
 * Update 6/10/2025 - Using this MutationObserver to handle initializing vote dial logic
 * Update 8/21/2025 - Using consolidated documentObserver
 */



/*
 There is an error where sometimes the card doesn't get initialized
 because dom elements are added before the observer is ready
 This is an attempt to stop that error (set 8-19-2025)
*/
document.addEventListener("DOMContentLoaded", () => {
    // First get all existing cards
    Array.from(document.querySelectorAll(".issue-card, .solution-card")).forEach((node) => {
        const issueId = node.id;
        initializeCard(issueId);
    })

    // Then listen for new cards to be added
    // The document observer makes it so you don't need to remember to set the event in the fetch. 
    // Simply add the content to the dom, and the dom will know what to do with it.
    if (typeof documentObserver == 'object') {
        documentObserver.registerEvent(initInitializeCardObserver)
    } else {
        throw error("documentObserver not defined")
    }

})


// Event registed in documentObservert event queue
function initInitializeCardObserver(node) {
    if (!(node instanceof Element)) return;
    if (node.classList && (node.classList.contains('issue-card') || node.classList.contains('solution-card'))) {
        // Card was added
        console.log('Issue card added via MutationObserver:', node);
        // Initialize card-specific JS here
        if (typeof initializeCard === 'function') {
            try {
                const contentId = node.id;
                initializeCard(contentId);
            } catch (initError) {
                console.error("Error in initializeVoteDial:", initError);
            }
        } else {
            console.error("initializeVoteDial function is not defined");
        }
    }
    const childCards = node.querySelectorAll('.issue-card, .solution-card');
    childCards.forEach(child => {
        const contentId = child.getAttribute("id")
        initializeCard(contentId);
    });
}

// The scope ribbon is getting an update 8/31/2025
// This sets a listener to simply add a class to the card ("show-composite-scope")
function initializeCompositeScopeRibbonListener(cardId) {
    const card = document.querySelector(`.card[id="${cardId}"]`)
    const ribbon = card.querySelector(".ribbon")

    ribbon.addEventListener("click", () => {
        card.classList.toggle("show-composite-scope")
    })
}



