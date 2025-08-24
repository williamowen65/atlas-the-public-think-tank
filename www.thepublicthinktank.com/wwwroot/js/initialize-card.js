
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

    if (!card.classList.contains("initialized")) {
    initializeVoteDial(cardId)
    // TBD - More initializations are possible
    }

    //card.classList.add("initialized")


}


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
    }

    setupQuickTabLinks(e)
})



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
    const saveVoteDebounced = createDebouncedSaveVote(issueId);
    const observer = configureIntersectionObserver(container, options, state, dialId);
    
    // Set up UI and event handlers
    scrollToSelectedOption(container, dialId);
    setupScrollEvents(container, dialId, state);
    setupRadioChangeEvents(radios, saveVoteDebounced, container, state);
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
        radio.addEventListener('change', function () {
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


    fetch(`/${contentType}/Vote`, {
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
        })
        .catch(error => {
            console.error('Error saving vote:', error);

                // Select dial number 5
                const defaultRadio = document.querySelector(`input[name="${dialId}"][value="5"]`);

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
        });
}

/**
 * Creates a debounced function for saving votes to the server
 */
function createDebouncedSaveVote(contentId) {
    const { container, dialId, contentType } = getDialElements(contentId);
    // Function to save vote to server with debouncing
    return debounce(function (voteValue) {
        saveVote(voteValue, {
            contentId,
            container,
            dialId,
            contentType
        })
    }, 800); // 800ms debounce delay
//
}


/**
 * Sets up intersection observer to track visible options during scrolling
 */
function configureIntersectionObserver(container, options, state, dialId) {
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
                    const saveVoteDebounced = createDebouncedSaveVote(issueId);
                    const { options, radios } = getDialElements(issueId);
                    // Recreate the reset method on the new container (recursive but will only happen once)
                    
                    scrollToSelectedOption(containerToReset, dialId);
                    setupScrollEvents(container, dialId, state);
                    setupRadioChangeEvents(radios, saveVoteDebounced, container, state);
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
    Array.from(document.querySelectorAll(".issue-card, .solution-card")).forEach((node) => {
        const issueId = node.id;
        initializeCard(issueId);
    })

    // Only do this after the inital loaded options have been set
    if (typeof documentObserver == 'object') {
        documentObserver.registerEvent(initInitializeCardObserver)
    } else {
        throw error("documentObserver not defined")
    }

})



function initInitializeCardObserver(node) {
        if (node.classList && (node.classList.contains('issue-card') || node.classList.contains('solution-card'))) {
            // Card was added
            //console.log('Issue card added via MutationObserver:', node);
            // Initialize card-specific JS here
            if (typeof initializeCard === 'function') {
                try {
                    const issueId = node.id;
                    initializeCard(issueId);
                } catch (initError) {
                    console.error("Error in initializeVoteDial:", initError);
                }
            } else {
                console.error("initializeVoteDial function is not defined");
            }

    }
}



