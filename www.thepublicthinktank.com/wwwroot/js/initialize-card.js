
/**
 * JavaScript file for issue/solution cards
 * 
 * This initializes the vote dial so that 
 * the lens is on the correct value, 
 * the lens follows the desired value, 
 * the dial can cast votes (Debounced - Wait until the dial has been still for a second before trying to vote)
 * 
 * Note: This file makes use of the IntersectionObserver API to track when a value changes during scroll.
 *
 */

function initializeCard(cardId) {
    initializeVoteDial(cardId)
    // TBD - More initializations are possible
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
})





// DIAL JS

function getDialElements(issueId) {
    const containerId = `vote-toggle-container-${issueId}`;
    const container = document.getElementById(containerId);

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
        container, dialId, options, radios
    }
}


/**
 * This is called multiple times on the same page for multiple dials
 * It knows where to add the dial to the DOM and will set all related events
 * @param {int} issueId - used to fetch the correct dial data
 * @returns void. 
 */
function initializeVoteDial(issueId) {
    console.log("Vote dial initialized for issue ID:", issueId);
    
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

/**
 * Creates a debounced function for saving votes to the server
 */
function createDebouncedSaveVote(contentId) {
    const { container, dialId } = getDialElements(contentId);
    // Function to save vote to server with debouncing
    return debounce(function(voteValue) {
        console.log(`Saving vote ${voteValue} for issue ${contentId}`);

        const contentType = container.getAttribute("data-content-type")

        const formData = new FormData();
        formData.append(`${contentType}ID`, contentId);
        formData.append('VoteValue', voteValue);
        
        const voteNotCasted = "Vote not casted"

        fetch(`/${contentType}/Vote`, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(async (response) => {
                if (!response.ok) {
                    throw new Error(voteNotCasted);
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

   if (error.message == voteNotCasted) {
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
                    <p>You need to be logged in to cast a vote.</p>
                    `,
                });

                client_CardFooter_Alert({
                    cardId: contentId,
                    type: 'plaintext',
                    message: `
                  Vote not cast - Login required
                    `,
                    dismissible: false
                });
            }, 500); 
            setTimeout(() => {
                container.classList.remove('temporarily-prevent-observer');
            }, 1000) // 1 second delay to allow scroll to finish
        }
    }
});
    }, 800); // 800ms debounce delay
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




