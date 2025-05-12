function getDialElements(forumId) {
    const containerId = `vote-toggle-container-${forumId}`;
    const container = document.getElementById(containerId);
    const dialId = `vote-dial-${forumId}`;
    const options = Array.from(container.querySelectorAll('.toggle-option'));
    const radios = Array.from(document.querySelectorAll(`input[name="${dialId}"]`));

    return {
        container, dialId, options, radios
    }
}

/**
 * This is called multiple times on the same page for multiple dials
 * It knows where to add the dial to the DOM and will set all related events
 * @param {int} forumId - used to fetch the correct dial data
 * @returns void. 
 */
function initializeVoteDial(forumId) {
    console.log("Vote dial initialized for forum ID:", forumId);
    
    // Get essential elements
    let {container, dialId} = getDialElements(forumId);
    if (!container) return;
    
    // Get options and radios
    const { options, radios } = getDialElements(forumId);

    
    
    // Create shared state object (passed by reference)
    const state = {
        isScrolling: false,
        scrollTimeout: null,
        isResetting: false
    };
    
    // Initialize components
    const saveVoteDebounced = createDebouncedSaveVote(forumId);
    const observer = configureIntersectionObserver(container, options, state, dialId);
    
    // Set up UI and event handlers
    scrollToSelectedOption(container, dialId);
    setupScrollEvents(container, dialId, state);
    setupRadioChangeEvents(radios, saveVoteDebounced, container, state);
    //createDialResetMethod(container, forumId, observer, dialId, options, state);
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
function createDebouncedSaveVote(forumId) {
    const { container, dialId } = getDialElements(forumId);
    // Function to save vote to server with debouncing
    return debounce(function(voteValue) {
        console.log(`Saving vote ${voteValue} for forum ${forumId}`);
        
        const formData = new FormData();
        formData.append('ForumID', forumId);
        formData.append('VoteValue', voteValue);
        
        const voteNotCasted = "Vote not casted"

        fetch('/Forum/Vote', {
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
            container.classList.add('user-voted');

            // update the vote average and the count
            const averageElement = document.querySelector(`#vote-average-${forumId}`);
            const countElement = document.querySelector(`#vote-count-${forumId}`);

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
                container.classList.remove('temporarily-prevent-observer');

                client_TopBar_Alert({
                    type: 'warning',
                    message: `
                    <h1>Vote not casted</h1> 
                    <p>You need to be logged in to cast a vote.</p>
                    `,
                     //timeout: 10000,
                     //dismissible: false
                });
                
            }, 1000);
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
function createDialResetMethod(container, forumId, observer, dialId, options, state) {
    container.reset = function (silent = false) {
        console.log(`Resetting vote dial for forum ${forumId}`);

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
                    const saveVoteDebounced = createDebouncedSaveVote(forumId);
                    const { options, radios } = getDialElements(forumId);
                    // Recreate the reset method on the new container (recursive but will only happen once)
                    
                    scrollToSelectedOption(containerToReset, dialId);
                    setupScrollEvents(container, dialId, state);
                    setupRadioChangeEvents(radios, saveVoteDebounced, container, state);
                    createDialResetMethod(containerToReset, forumId, observer, dialId, options, state);
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
 * Creates a debounced version of a function
 * @param {Function} func - The function to debounce
 * @param {number} wait - The number of milliseconds to delay
 * @returns {Function} - The debounced function
 */
function debounce(func, wait) {
    let timeout;
    return function(...args) {
        const context = this;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), wait);
    };
}